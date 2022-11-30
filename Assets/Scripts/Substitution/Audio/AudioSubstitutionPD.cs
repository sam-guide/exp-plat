using System;
using UnityEngine;
using UnityEngine.UI;
using static Substitution.Transcoding;

namespace Substitution {

    public class AudioSubstitutionPD : AudioSubstitution {
        
        private LibPdConnector pdConnector;
        public string patchName {get; protected set;}

        public void Init(
            string _distanceEncoding = "Intensity", float? _distanceMin = 1f, float? _distanceMax = 30f, 
            string _distanceToIntensityLink = "Linear", float? _intensityMin = 0.01f, float? _intensityMax = 1f, int? _intensitySteps = 0,
            string _distanceToIBILink = "Linear", float? _IBIMin = 1f, float? _IBIMax = 1/20f, int? _IBISteps = 0,
            string _angleEncoding = "Frequency", float? _angleMin = 1f, float? _angleMax = 180f,
            string _angleToFrequencyLink = "Linear", float? _frequencyMin = 200f, float? _frequencyMax = 800f, int? _frequencySteps = 0,
            string patchName = "test2"
        )
        {
            this.patchName = "/PdAssets/" + patchName;

            base.Init(
                _distanceEncoding, _distanceMin, _distanceMax, 
                _distanceToIntensityLink, _intensityMin, _intensityMax, _intensitySteps,
                _distanceToIBILink, _IBIMin, _IBIMax, _IBISteps,
                _angleEncoding, _angleMin, _angleMax,
                _angleToFrequencyLink, _frequencyMin, _frequencyMax, _frequencySteps
            );
        }
        
        protected override void Awake()
        {
            base.Awake();

            if ((distanceToIBI || distanceToIntensity) && (angleToFrequency || angleToStereo)) initDone = true;
            if (!initDone) Init();

            /** Setting <PureData> parameters **/
            pdConnector = gameObject.GetComponent<LibPdConnector>() == null ? gameObject.AddComponent<LibPdConnector>() : gameObject.GetComponent<LibPdConnector>();

            /** Setting AudioSource parameters **/
            audioSource.spatialBlend = 1.0f;

            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSource.minDistance = 1;
            audioSource.maxDistance = 5;

            /** Setting <SteamAudioSource> parameters **/
            steamAudioSource.directMixLevel = 0.7f; // FIXME: temporary fix for the grésillements
        }

        protected override void Start()
        {
            base.Start();

            pdConnector.SendFloat("left", 1);
            pdConnector.SendFloat("right", 1);
            pdConnector.SendFloat("freq", frequencyMin);

            audioSource.Play();
        }

        protected override void Update()
        {
            base.Update();

            if (audioSource.isPlaying)
            {
                /** Distance encoding **/
                if (distanceToIntensity) pdConnector.SendFloat("gain", intensity);

                /** Angle encoding **/
                if (angleToFrequency) pdConnector.SendFloat("freq", (float)frequency);
            }
        }
    }
}