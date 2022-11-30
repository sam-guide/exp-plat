using UnityEngine;
using SteamAudio;

namespace Substitution {

    public class AudioSubstitutionClip : AudioSubstitution
    {
        [SerializeField, ReadOnly] private AudioClip clip = null;

        public void Init(string _clipName = "Dirty Swamp Jack")
        {
            clip = Resources.Load<AudioClip>("Sounds/" + _clipName);

            base.Init(_distanceEncoding: "None", _angleEncoding: "None");

            initDone = true;
        }

        protected override void Awake()
        {
            base.Awake();

            if (clip != null) initDone = true;
            if (!initDone) Init("Dirty Swamp Jack");

            /** Setting <AudioSource> parameters **/
            audioSource.clip = clip;
            audioSource.volume = 1f;
            audioSource.spatialBlend = 1.0f;
            audioSource.loop = true;

            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSource.minDistance = 1;
            audioSource.maxDistance = 20;

            /** Setting <SteamAudioSource> parameters **/
            steamAudioSource.distanceAttenuation = true;
            // steamAudioSource.occlusion = true; // Handled by the BeaconController
            // steamAudioSource.transmission = true;
        }

        protected override void Start()
        {
            base.Start();

            audioSource.Play();
        }

        protected override void Update()
        {
            base.Update();
        }
    }
}