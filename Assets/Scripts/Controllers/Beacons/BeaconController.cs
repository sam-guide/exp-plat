using UnityEngine;
using SteamAudio;
using Substitution;

namespace Controllers.Beacons {

    public enum BeaconType { Target, Landmark, Waypoint, Center };

    public class BeaconController : MonoBehaviour
    {
        [SerializeField] protected bool isVisible = false;
        [ReadOnly] public FeedbackModality modality; // TODO: make it a list for bimodal feedback ?

        [ReadOnly] public TactileSubstitution tactileSubstitution;
        
        [ReadOnly] public AudioSubstitution audioSubstitution;
        [ReadOnly] public AudioSource audioSource;
        protected SteamAudioSource steamAudioSource;

        // Identity of the Beacon
        [field: SerializeField, ReadOnly] public AudioClip triggerSound { get; protected set; }
        // [field: SerializeField, ReadOnly] public string pattern { get; protected set; } // TODO: specific Tactile pattern of that beacon

        protected virtual void Awake()
        {
            gameObject.tag = "Beacon";
            gameObject.GetComponent<Renderer>().enabled = isVisible;

            if (modality == FeedbackModality.Audio)
            {
                audioSubstitution = GetComponent<AudioSubstitution>() == null ? gameObject.AddComponent<AudioSubstitution>() : GetComponent<AudioSubstitution>();
                audioSource = GetComponent<AudioSource>() == null ? gameObject.AddComponent<AudioSource>() : GetComponent<AudioSource>();
                steamAudioSource = GetComponent<SteamAudioSource>() == null ? gameObject.AddComponent<SteamAudioSource>() : GetComponent<SteamAudioSource>();
            }
            if (modality == FeedbackModality.Tactile)
            {
                tactileSubstitution = GetComponent<TactileSubstitution>() == null ? gameObject.AddComponent<TactileSubstitution>() : GetComponent<TactileSubstitution>();
            }
        }

        public virtual void OnTrigger()
        {
            
        }

        void OnValidate()
        {
            gameObject.GetComponent<Renderer>().enabled = isVisible;
        }

        public SubstitutionSource GetSubstitutionSource()
        {
            SubstitutionSource source = this.modality == FeedbackModality.Audio ? this.audioSubstitution : this.tactileSubstitution;
            return(source);
        }
    }
}