using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using static OSC;
using static Utils.Spatial;
using static Substitution.Transcoding;

namespace Test {

    public enum DistanceEncoding {Intensity, IBI, None};
    public enum Motors {_00, _01, _02, _03, _04, _05, _06, _07, _08, _09, _10, _11, _12, _13, _14, _15, _all};

    #if (UNITY_EDITOR)
    [CustomEditor(typeof(BeltTest), true)]
    public class BeltTestCustomEditor : Editor {

        public override void OnInspectorGUI() {
            serializedObject.Update();

            BeltTest bt = (BeltTest)target;
            if(GUILayout.Button("Victory")) {
                bt.InitiateVictoryCoroutine();
            }

            DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();
        }
    }
    #endif

    public class BeltTest : MonoBehaviour
    {
        private OSC osc;
        
        public Motors currentMotor = Motors._00;
        private Motors previousMotor = Motors._00;

        public DistanceEncoding mapping = DistanceEncoding.None;
        [Range(0,255)] public int intensity = 0;
        private int _intensity;
        private bool intensityChanged = true;
        [Range(0,1.5f)] public float IBI = 0.5f;

        private IEnumerator currentIBICoroutine;


        void Awake()
        {
            osc = GetComponent<OSC>() == null ? gameObject.AddComponent<OSC>() : GetComponent<OSC>();
        }

        void Start()
        {
            SendIntensity("all", 0); // Reset all motors
            Debug.Log(((Motors)0).ToString());
        }

        void Update()
        {
            if (mapping == DistanceEncoding.None)
            {
                SendIntensity("all", 0); // Reset all motors
                if(currentIBICoroutine != null)
                {
                    StopCoroutine(currentIBICoroutine);
                    currentIBICoroutine = null;
                }
            }

            /** Distance encoding **/
            if (mapping == DistanceEncoding.Intensity)
            {
                if (previousMotor != currentMotor) SendIntensity(previousMotor.ToString().Substring(1), 0); // Make sure the previous motor is off
                if (intensityChanged) {
                    Debug.Log("Updating motor intensity");
                    SendIntensity(currentMotor.ToString().Substring(1), intensity);
                    intensityChanged = false;
                }
            }

            if (mapping == DistanceEncoding.IBI)
            {
                if(currentIBICoroutine == null) InitiateIBICoroutine(); // We only need to start it once, the coroutine is self-maintained
            }
            else
            {
                if(currentIBICoroutine != null)
                {
                    StopCoroutine(currentIBICoroutine);
                    currentIBICoroutine = null;
                }
            }
            previousMotor = currentMotor;
        }

        void OnValidate()
        {
            if (_intensity != intensity)
            {
                _intensity = intensity;
                intensityChanged = true;
            }
        }

        void OnDisable()
        {
            SendIntensity("all", 0); // Reset all motors
            StopAllCoroutines();
        }

        private void SendIntensity(string id, int intensity)
        {
            OscMessage message = new OscMessage();
            message.address = "/" + id;
            message.values.Add(intensity);
            osc.Send(message);
        }

        private void InitiateIBICoroutine()
        {
            if (previousMotor != currentMotor) SendIntensity(previousMotor.ToString().Substring(1), 0); // Make sure the previous motor is off
            if(currentIBICoroutine != null) StopCoroutine(currentIBICoroutine);
            currentIBICoroutine = IBICoroutine(currentMotor.ToString().Substring(1), intensity, (float)IBI);
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

        public void InitiateVictoryCoroutine()
        {
            StartCoroutine(VictoryCoroutine());
        }

        private IEnumerator VictoryCoroutine()
        {
            for (int i = 1; i <= 3; i++) 
            {
                SendIntensity("all", 200);
                yield return new WaitForSeconds(0.3f);
                SendIntensity("all", 0);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}