using UnityEngine;

namespace Controllers.Beacons
{
    public class WaypointBeaconController : BeaconController
    {
        protected override void Awake()
        {
            base.Awake();

            if (this.triggerSound == null) this.triggerSound = Resources.Load<AudioClip>("Sounds/coin"); 
            // this.triggerSound ??= Resources.Load<AudioClip>("Sounds/coin");
            
            gameObject.GetComponent<Collider>().isTrigger = true;

            this.steamAudioSource.occlusion = true;
        }

        public override void OnTrigger()
        {
            base.OnTrigger();
            this.audioSource.Stop();
        }
    }
}