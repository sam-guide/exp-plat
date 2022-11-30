using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SteamAudio;
using Utils;
using static Utils.Spatial;
using static Substitution.Transcoding;

namespace Substitution
{

    public class AudioSubstitution : SubstitutionSource
    {
        
        #region Properties

        protected AudioSource audioSource;
        protected SteamAudioSource steamAudioSource;
        [ReadOnly] public AudioFeedbackType audioFeedbackType;
        
        [Header("Distance mapping")]
        // [ReadOnly] public float distanceToPlayer;
        [Range(0,100f)] public float distanceMin;
        [Range(0,100f)] public float distanceMax;

        [Space(2)]
        public bool distanceToIntensity;
        public LinkType distanceToIntensityLink;
        [ReadOnly] public float intensity;
        [Range(0.01f,1)] public float intensityMin;
        [Range(0.01f,1)] public float intensityMax;
        [Tooltip("0 is continuous output, and >= 1 is for automatic n-step discrete breaks")]
        [Range(0,30)] public int intensitySteps;

        [Space(2)]
        public bool distanceToIBI;
        public LinkType distanceToIBILink;
        [ReadOnly] public double IBI;
        [Range(0,30f)] public float IBIMin;
        [Range(0,30f)] public float IBIMax;
        [Tooltip("0 is continuous output, and >= 1 is for automatic n-step discrete breaks")]
        [Range(0,30)] public int IBISteps;

        [Space(10)]

        [Header("Angle mapping")]
        // [ReadOnly] public float angleToPlayer;
        [Range(0,180f)] public float angleMin;
        [Range(0,180f)] public float angleMax;

        [Space(2)]
        public bool angleToFrequency = false;
        public LinkType angleToFrequencyLink;
        [ReadOnly] public double frequency;
        [Range(0,1500f)] public float frequencyMin;
        [Range(0,1500f)] public float frequencyMax;
        [Tooltip("0 is continuous output, and >= 1 is for automatic n-step discrete breaks")]
        [Range(0,30)] public int frequencySteps;

        [Space(2)]
        public bool angleToStereo = false;
        public LinkType angleToStereoLink;
        [ReadOnly] public float stereoMultLeft;
        [ReadOnly] public float stereoMultRight;
        [Range(0.01f,1.0f)] public float stereoMultMin;
        [Range(0.01f,1.0f)] public float stereoMultMax;
        [Tooltip("0 is continuous output, and >= 1 is for automatic n-step discrete breaks")]
        [Range(0,30)] public int stereoSteps;
        
        [Space(5)]

        public string stepAlong;
        // public bool center = false; // TODO

        /** Link function samples **/
        protected List<double> distanceToIntensityBreaks;
        protected List<double> distanceToIBIBreaks;
        protected List<double> angleToFrequencyBreaks;
        protected List<double> angleToStereoBreaks;
        protected List<double> midpoints;

        protected bool initDone = false;
        
        #endregion
        
        public virtual void Init(
            string _distanceEncoding = "Intensity", float? _distanceMin = 0.5f, float? _distanceMax = 100f, 
            string _distanceToIntensityLink = "Linear", float? _intensityMin = 0.01f, float? _intensityMax = 1f, int? _intensitySteps = 0,
            string _distanceToIBILink = "Linear", float? _IBIMin = 1f, float? _IBIMax = 1/20f, int? _IBISteps = 0,
            string _angleEncoding = "Frequency", float? _angleMin = 1f, float? _angleMax = 180f,
            string _angleToFrequencyLink = "Linear", float? _frequencyMin = 200f, float? _frequencyMax = 800f, int? _frequencySteps = 0,
            string _stepAlong = "x"
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
            this.distanceMax = _distanceMax ?? 30f;

            this.distanceToIntensityLink = (LinkType) Enum.Parse(typeof(LinkType), _distanceToIntensityLink ?? "Linear", true);
            this.intensityMin = _intensityMin ?? 0.01f;
            this.intensityMax = _intensityMax ?? 1f;
            this.intensitySteps = _intensitySteps ?? 0;

            this.distanceToIBILink = (LinkType) Enum.Parse(typeof(LinkType), _distanceToIBILink ?? "Linear", true);
            this.IBIMin = _IBIMin ?? 1f;
            this.IBIMax = _IBIMax ?? 1/20f;
            this.IBISteps = _IBISteps ?? 0;

            AngleEncoding __angleEncoding = (AngleEncoding) Enum.Parse(typeof(AngleEncoding), _angleEncoding ?? "None", true);
            switch(__angleEncoding)
            {
                case AngleEncoding.None:
                    this.angleToFrequency = false;
                    this.angleToStereo = false;
                    break;
                case AngleEncoding.Frequency:
                    this.angleToFrequency = true;
                    break;
                case AngleEncoding.Stereo:
                    this.angleToStereo = true;
                    break;
            }
            this.angleMin = _angleMin ?? 1f;
            this.angleMax = _angleMax ?? 180f;

            this.angleToFrequencyLink = (LinkType) Enum.Parse(typeof(LinkType), _angleToFrequencyLink ?? "Linear", true);
            this.frequencyMin = _frequencyMin ?? 200f;
            this.frequencyMax = _frequencyMax ?? 800f;
            this.frequencySteps = _frequencySteps ?? 0;

            this.stepAlong = _stepAlong ?? "x";

            initDone = true;
        }

        #region MonoBehaviour methods

        protected override void Awake()
        {
            base.Awake();

            /** Setting <AudioSource> parameters **/
            audioSource = gameObject.GetComponent<AudioSource>() == null ? gameObject.AddComponent<AudioSource>() : gameObject.GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialize = true;
            audioSource.priority = 0;
            audioSource.dopplerLevel = 0;
            audioSource.spread = 0;

            /** Setting <SteamAudioSource> parameters **/
            steamAudioSource = gameObject.GetComponent<SteamAudioSource>() == null ? gameObject.AddComponent<SteamAudioSource>() : gameObject.GetComponent<SteamAudioSource>();  
        }

        protected override void Start()
        {
            base.Start();

            /** Sampling link functions steps **/
            UpdateBreaks();
        }

        protected override void Update()
        {
            base.Update();

            if (audioSource.isPlaying)
            {
                /** Distance encoding **/
                if (distanceToIntensity) intensity = (float)Map(distanceToIntensityLink, distanceToPlayer, distanceMin, intensityMax, distanceMax, intensityMin, intensitySteps, distanceToIntensityBreaks);
                else intensity = intensityMax;
                audioSource.volume = intensity;

                if(distanceToIBI) IBI = Map(distanceToIBILink, distanceToPlayer, distanceMin, IBIMax, distanceMax, IBIMin, IBISteps, distanceToIBIBreaks);

                /** Angle encoding **/
                if (angleToFrequency) frequency = Map(angleToFrequencyLink, Mathf.Abs(angleToPlayer), angleMin, frequencyMax, angleMax, frequencyMin, frequencySteps, angleToFrequencyBreaks);
                else frequency = (new List<float>{ frequencyMin, frequencyMax }).Average();
            
                if (angleToStereo)
                {
                    stereoMultLeft = (float)Map(angleToStereoLink, angleToPlayer, angleMin, stereoMultMax, angleMax, stereoMultMin, stereoSteps, angleToStereoBreaks);
                    stereoMultRight = (float)Map(angleToStereoLink, -1 * angleToPlayer, angleMin, stereoMultMax, angleMax, stereoMultMin, stereoSteps, angleToStereoBreaks);
                }
            }
        }

        void OnDisable()
        {
            audioSource.Stop();
        }

        void OnDestroy()
        {
            audioSource.Stop();
        }

        #endregion

        /***************************[ Updating sound paramters ]***************************/

        public void UpdateBreaks()
        {
            if(distanceToIntensity && intensitySteps > 0) distanceToIntensityBreaks = GetBreaks(distanceToIntensityLink, distanceToPlayer, distanceMin, intensityMax, distanceMax, intensityMin, intensitySteps, stepAlong);
            if(distanceToIBI && IBISteps > 0) distanceToIBIBreaks = GetBreaks(distanceToIBILink, distanceToPlayer, distanceMin, IBIMax, distanceMax, IBIMin, IBISteps, stepAlong);
            if(angleToFrequency && frequencySteps > 0) angleToFrequencyBreaks = GetBreaks(angleToFrequencyLink, Mathf.Abs(angleToPlayer), angleMin, frequencyMax, angleMax, frequencyMin, frequencySteps, stepAlong);
            if(angleToStereo && stereoSteps > 0) angleToStereoBreaks = GetBreaks(angleToStereoLink, angleToPlayer, angleMin, stereoMultMax, angleMax, stereoMultMin, stereoSteps, stepAlong);
        }

        public override DistanceEncoding GetDistanceEncoding()
        {
            if (this.distanceToIBI) return(DistanceEncoding.IBI);
            else if (this.distanceToIntensity) return(DistanceEncoding.Intensity);
            else return(DistanceEncoding.None);
        }

        public override AngleEncoding GetAngleEncoding()
        {
            if (this.angleToFrequency) return(AngleEncoding.Frequency);
            else if (this.angleToStereo) return(AngleEncoding.Stereo);
            else return(AngleEncoding.None);
        }
    }
}