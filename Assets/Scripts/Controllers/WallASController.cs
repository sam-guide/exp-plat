using UnityEngine;
using SteamAudio;

namespace Controllers {
    public class WallASController : MonoBehaviour
    {
        private AudioSource audioSource;
        private SteamAudioSource steamAudioSource;
        private AudioClip proximitySound;

        void Awake()
        {
            // if (gameObject.GetComponent<Collider>() == null) gameObject.AddComponent<BoxCollider>();
            // gameObject.tag = "Wall";
 
            audioSource = gameObject.GetComponent<AudioSource>() == null ? gameObject.AddComponent<AudioSource>() : gameObject.GetComponent<AudioSource>();

            proximitySound ??= Resources.Load<AudioClip>("Sounds/Danger");

            audioSource.clip = proximitySound;
            audioSource.priority = 5;
            audioSource.spatialize = true; // Custom spatializer effects improve the realism of sound propagation by incorporating the binaural head-related transfer function (HRTF)
            audioSource.spatialBlend = 1.0f;
            audioSource.panStereo = 0.0f; // -1.0 (full left) to 1.0 (full right)
            audioSource.spread = 0; // 0 = all sound channels are located at the same speaker location and is 'mono'. 360 = all subchannels are located at the opposite speaker location to the speaker location that it should be according to 3D position.
            audioSource.dopplerLevel = 0;
            audioSource.playOnAwake = true;
            audioSource.loop = true;

            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = 0.5f;
            audioSource.maxDistance = 1.5f;

            /** Setting <SteamAudioSource> parameters **/
            steamAudioSource = gameObject.GetComponent<SteamAudioSource>() == null ? gameObject.AddComponent<SteamAudioSource>() : gameObject.GetComponent<SteamAudioSource>();  

            steamAudioSource.distanceAttenuation = true;
            // steamAudioSource.occlusion = false;
        }

        void Start()
        {
            audioSource.Play();
        }

        void Update()
        {
            
        }

        /* void OnTriggerEnter(Collider other) {
            if (other.gameObject.CompareTag("Player")) other.GetComponent<AudioSource>().PlayOneShot(triggerSound, 0.2f); // FIXME: works better if Triggered from the PlayerController           
        } */
    }
}