using System;
using UnityEngine;
using UnityEngine.UI;
using Waves;
using static Substitution.Transcoding;

namespace Substitution {

    public class AudioSubstitutionManual : AudioSubstitution {
        
        #region Properties
        private SinWave sinAudioWave; // Base sound wave
        private SquareWave amplitudeModulationOscillator; // Used to make the biping sound
        private double samplingRate;
        private double dataLen;
        private double chunkTime;
        private double dspTimeStep;
        private double currentDspTime;
        private bool isPlaying = false;

        #endregion

        public void Init(
            string _distanceEncoding = "IBI", float? _distanceMin = 1f, float? _distanceMax = 30f, 
            string _distanceToIntensityLink = "Linear", float? _intensityMin = 0.01f, float? _intensityMax = 1f, int? _intensitySteps = 0,
            string _distanceToIBILink = "Linear", float? _IBIMin = 1f, float? _IBIMax = 1/20f, int? _IBISteps = 0,
            string _angleEncoding = "Frequency", float? _angleMin = 1f, float? _angleMax = 180f,
            string _angleToFrequencyLink = "Linear", float? _frequencyMin = 200f, float? _frequencyMax = 800f, int? _frequencySteps = 0
        )
        {

            base.Init(
                _distanceEncoding, _distanceMin, _distanceMax, 
                _distanceToIntensityLink, _intensityMin, _intensityMax, _intensitySteps,
                _distanceToIBILink, _IBIMin, _IBIMax, _IBISteps,
                _angleEncoding, _angleMin, _angleMax,
                _angleToFrequencyLink, _frequencyMin, _frequencyMax, _frequencySteps
            );
        }

        #region MonoBehaviour methods

        protected override void Awake()
        {
            base.Awake();

            if ((distanceToIBI || distanceToIntensity) && (angleToFrequency || angleToStereo)) initDone = true;
            if (!initDone) Init();

            /** Init wave generators **/
            sinAudioWave = new SinWave();
            amplitudeModulationOscillator = new SquareWave();
            samplingRate = AudioSettings.outputSampleRate;

            /** Setting Substitution AudioSource parameters **/
            audioSource.spatialize = true;
            audioSource.spatialBlend = 0.0f;

            /** Setting <SteamAudioSource> parameters **/
            // steamAudioSource.distanceAttenuation = true;
            // steamAudioSource.occlusion = true; // TODO: depends on BeaconType
        }

        protected override void Start()
        {
            base.Start();

            audioSource.Play();
            isPlaying = audioSource.isPlaying;
        }

        protected override void Update()
        {
            base.Update();

            isPlaying = audioSource.isPlaying;
        }

        /** Ressources:
        * - https://www.youtube.com/watch?v=GqHFGMy_51c&t=129s
        * - https://github.com/konsfik/Unity3D-Coding-Examples/tree/master/3-Procedural-Audio/ProceduralAudioUnityProject/Assets/Scripts
        */
        void OnAudioFilterRead(float[] data, int channels)
        {
            if (!isPlaying)
                return;

            currentDspTime = AudioSettings.dspTime;
            dataLen = data.Length / channels;	// Number of samples per channel,
            chunkTime = dataLen / samplingRate;	// The time that each chunk of data lasts
            dspTimeStep = chunkTime / dataLen;	// The time of each dsp step. (the time that each individual audio sample (actually a float value) lasts)
            double preciseDspTime;

            for (int i = 0; i < dataLen; i++)
            {
                preciseDspTime = currentDspTime +  i * dspTimeStep;
                double signalValue = 0.0;

                /** Creating the base wave **/
                signalValue += 1.0f * sinAudioWave.calculateSignalValue(preciseDspTime, frequency);

                /** Modulating the amplitude **/
                if (distanceToIBI) signalValue *= Math.Max(LinearLink(amplitudeModulationOscillator.calculateSignalValue(preciseDspTime, IBI), -1.0, 0.0, 1.0, 1.0), 0);
                if (distanceToIntensity) signalValue *= intensity;

                /** Stereo **/
                data[i * channels] = angleToStereo ? (float)signalValue * stereoMultLeft : (float)signalValue; // Left speaker
                if (channels == 2) data[i * channels + 1] = angleToStereo ? (float)signalValue * stereoMultRight : (float)signalValue; // Right speaker
            }
        }

        #endregion
    }
}