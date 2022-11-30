using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static OSC;
using static Utils.Spatial;
using static Substitution.Transcoding;

namespace Substitution {

    /** TODO: 
    - [Future] Limit max number of Tactile sources (= store them all in a list)
    **/
    public class TactileSubstitution : SubstitutionSource
    {
        #region Properties
        
        protected OSC osc;

        // TODO: patterns [.-.][-.-][-..][..-] --> in each BeaconType ? (audio pattern too --> different bip base sound)
        
        [Header("Distance mapping")]
        // [ReadOnly] public float distanceToPlayer;
        [Range(0,100f)] public float distanceMin = 0.5f;
        [Range(0,100f)] public float distanceMax = 30f;

        [Space(2)]
        public bool distanceToIntensity = true;
        public LinkType distanceToIntensityLink;
        [ReadOnly] public int intensity;
        private int _previousIntensity;
        [Range(0.01f,255f)] public float intensityMin = 0.01f;
        [Range(0.01f,255f)] public float intensityMax = 255f;
        [Tooltip("0 is continuous output, and >= 1 is for automatic n-step discrete breaks")]
        [Range(0,30)] public int intensitySteps = 0;

        [Space(2)]
        public bool distanceToIBI = false;
        public LinkType distanceToIBILink;
        [ReadOnly] public double IBI;
        private double _previousIBI;
        [Range(0,30f)] public float IBIMin = 1/1f;
        [Range(0,30f)] public float IBIMax = 1/20f;
        [Tooltip("0 is continuous output, and >= 1 is for automatic n-step discrete breaks")]
        [Range(0,30)] public int IBISteps = 0;

        [Space(10)]

        [Header("Angle mapping")]
        // [ReadOnly] public float angleToPlayer;
        [ReadOnly] public string currentMotor = "00";
        private string _previousMotor;
        [Range(0,180f)] public float angleMin = 1f;
        [Range(0,180f)] public float angleMax = 180f;

        private List<string> motorOrder = new List<string>{"00", "01", "02", "03", "04", "05", "06", "07", "15", "14", "13", "12", "11", "08", "10", "09"};
        private Dictionary<string, Tuple<double, double>> motorAngleMap;

        [Space(10)]

        /** Link function discrete samples **/
        public string stepAlong = "x";

        protected List<double> distanceToIntensityBreaks;
        protected List<double> distanceToIBIBreaks;

        protected bool initDone = false;

        private IEnumerator currentIBICoroutine;

        #endregion

        public void Init(
            List<string> _motorOrder,
            string _distanceEncoding = "None", float? _distanceMin = 0.5f, float? _distanceMax = 20f, 
            string _distanceToIntensityLink = "Linear", float? _intensityMin = 0.01f, float? _intensityMax = 255f, int? _intensitySteps = 0,
            string _distanceToIBILink = "Linear", float? _IBIMin = 1/1f, float? _IBIMax = 1/20f, int? _IBISteps = 0,
            float? _angleMin = 1f, float? _angleMax = 180f
        )
        {
            DistanceEncoding __distanceEncoding = (DistanceEncoding) Enum.Parse(typeof(DistanceEncoding), _distanceEncoding ?? "None", true);
            switch(__distanceEncoding)
            {
                case DistanceEncoding.None:
                    this.distanceToIntensity = false;
                    this.distanceToIBI = false;
                    break;
                case DistanceEncoding.Intensity:
                    this.distanceToIntensity = true;
                    this.distanceToIBI = false;
                    break;
                case DistanceEncoding.IBI:
                    this.distanceToIntensity = false;
                    this.distanceToIBI = true;
                    break;
            }
            this.distanceMin = _distanceMin ?? 1f;
            this.distanceMax = _distanceMax ?? 20f;

            this.distanceToIntensityLink = (LinkType) Enum.Parse(typeof(LinkType), _distanceToIntensityLink ?? "Linear", true);
            this.intensityMin = _intensityMin ?? 0.01f;
            this.intensityMax = _intensityMax ?? 255f;
            this.intensitySteps = _intensitySteps ?? 0;

            this.distanceToIBILink = (LinkType) Enum.Parse(typeof(LinkType), _distanceToIBILink ?? "Linear", true);
            this.IBIMin = _IBIMin ?? 1/1f;
            this.IBIMax = _IBIMax ?? 1/20f;
            this.IBISteps = _IBISteps ?? 0;

            this.motorOrder = _motorOrder ?? new List<string>{"00", "01", "02", "03", "04", "05", "06", "07", "15", "14", "13", "12", "11", "08", "10", "09"};

            initDone = true;
        }

        /** ====================================== **/
        #region MonoBehaviour methods

        protected override void Awake()
        {
            base.Awake();

            osc = GetComponent<OSC>() == null ? gameObject.AddComponent<OSC>() : GetComponent<OSC>();

            motorAngleMap = ComputeAngleSegments();
            // motorAngleMap.ToList().ForEach(x => Debug.Log("{" + x.Key + " : " + x.Value + "}"));
        }

        protected override void Start()
        {
            base.Start();

            SendIntensity("all", 0); // Reset all motors
            
            UpdateSpatialParameters();

            UpdateBreaks(); // Initial sampling of link functions
        }

        protected override void Update()
        {
            base.Update();

            UpdateSpatialParameters();

            /** Distance encoding **/
            if (distanceToIntensity)
            {
                _previousIntensity = intensity;
                intensity = (int)Math.Round(
                    Map(distanceToIntensityLink, distanceToPlayer, distanceMin, intensityMax, distanceMax, intensityMin, intensitySteps, distanceToIntensityBreaks)
                );

                if (_previousMotor != currentMotor)
                {
                    SendIntensity(_previousMotor, 0); // Make sure the previous motor is off
                    SendIntensity(currentMotor, intensity);
                }
                // Same motor but intensity change
                else
                {
                    if (_previousIntensity != intensity) SendIntensity(currentMotor, intensity);
                }             
            }

            if (distanceToIBI)
            {
                intensity = 255;
                // intensity = (int)Math.Round((new List<float>{ intensityMin, intensityMax }).Average());
                IBI = Map(distanceToIBILink, distanceToPlayer, distanceMin, IBIMax, distanceMax, IBIMin, IBISteps, distanceToIBIBreaks);
                
                if(currentIBICoroutine == null) InitiateIBICoroutine(); // We only need to start it once, the coroutine is self-maintained
            }
        }

        void OnDisable()
        {
            SendIntensity("all", 0); // Reset all motors
            StopAllCoroutines();
        }

        #endregion

        /** ====================================== **/
        #region TactiBelt methods

        private void SendIntensity(string id, int intensity)
        {
            OscMessage message = new OscMessage();
            message.address = "/" + id;
            message.values.Add(intensity);
            osc.Send(message);
        }

        private void InitiateIBICoroutine()
        {
            SendIntensity(_previousMotor, 0); // Make sure the previous motor is off
            if(currentIBICoroutine != null) StopCoroutine(currentIBICoroutine);
            currentIBICoroutine = IBICoroutine(currentMotor, intensity, (float)IBI);
            StartCoroutine(currentIBICoroutine);
        }

        private IEnumerator IBICoroutine(string id, int intensity, float durationOff, float durationOn = 0.5f)
        {
            SendIntensity(id, intensity);

            yield return new WaitForSeconds(durationOn);

            SendIntensity(id, 0);

            yield return new WaitForSeconds(durationOff);

            InitiateIBICoroutine();
        }

        #endregion

        private void UpdateSpatialParameters()
        {
            double angleToPlayer360 = (double)(angleToPlayer < 0 ? 360 + angleToPlayer : angleToPlayer);
            
            _previousMotor = currentMotor;
            currentMotor = motorAngleMap.Where(
                e => angleToPlayer360 >= motorAngleMap.First().Value.Item1 || angleToPlayer360 < motorAngleMap.First().Value.Item2 ? 
                        e.Key == "00" : 
                        angleToPlayer360 >= e.Value.Item1 && angleToPlayer360 < e.Value.Item2
            ).First().Key;
        }

        public void UpdateBreaks()
        {
            if(distanceToIntensity && intensitySteps > 0) distanceToIntensityBreaks = GetBreaks(distanceToIntensityLink, distanceToPlayer, distanceMin, intensityMax, distanceMax, intensityMin, intensitySteps, stepAlong);
            if(distanceToIBI && IBISteps > 0) distanceToIBIBreaks = GetBreaks(distanceToIBILink, distanceToPlayer, distanceMin, IBIMax, distanceMax, IBIMin, IBISteps, stepAlong);
        }

        public override DistanceEncoding GetDistanceEncoding()
        {
            if (this.distanceToIBI) return(DistanceEncoding.IBI);
            else if (this.distanceToIntensity) return(DistanceEncoding.Intensity);
            else return(DistanceEncoding.None);
        }

        public override AngleEncoding GetAngleEncoding()
        {
            return(AngleEncoding.None);
        }

        private Dictionary<string, Tuple<double, double>> ComputeAngleSegments()
        {
            double hw = 360.0 / (this.motorOrder.Count() * 2.0);
            List<double> splits = Enumerable.Range(0, this.motorOrder.Count()).Select(x => ((x * 2) + 1) * (hw)).ToList();
            splits.Insert(0, 360 - hw);

            List<Tuple<double, double>> segments = Enumerable.Range(0, this.motorOrder.Count()).Select(i => Tuple.Create(splits[i], splits[i+1])).ToList();

            // Debug.Log("Segment size: " + segments.Count());
            // segments.ForEach(x => Debug.Log(x));

            return(
                motorOrder.Select((k, i) => new { k, v = segments[i] }).ToDictionary(x => x.k, x => x.v)
                /* new Dictionary<string, Tuple<double, double>>()
                {
                    {"00", Tuple.Create(348.75, 11.25)},
                    {"01", Tuple.Create(11.25, 33.75)},
                    {"02", Tuple.Create(33.75, 56.25)},
                    {"03", Tuple.Create(56.25, 78.75)},
                    {"04", Tuple.Create(78.75, 101.25)},
                    {"05", Tuple.Create(101.25, 123.75)},
                    {"06", Tuple.Create(123.75, 146.25)},
                    {"07", Tuple.Create(146.25, 168.75)},
                    {"08", Tuple.Create(168.75, 191.25)},
                    {"09", Tuple.Create(191.25, 213.75)},
                    {"10", Tuple.Create(213.75, 236.25)},
                    {"11", Tuple.Create(236.25, 258.75)},
                    {"12", Tuple.Create(258.75, 281.25)},
                    {"13", Tuple.Create(281.25, 303.75)},
                    {"14", Tuple.Create(303.75, 326.25)},
                    {"15", Tuple.Create(326.25, 348.75)}
                } */
            );
        }
    }
}