using UnityEngine;

namespace Controllers.Beacons
{
    public class TargetBeaconController : BeaconController
    {
        protected override void Awake()
        {
            base.Awake();

            gameObject.tag = "Pickup"; // TODO: remove this specific tag

            if (this.triggerSound == null) this.triggerSound = Resources.Load<AudioClip>("Sounds/money"); 
            // this.triggerSound ??= Resources.Load<AudioClip>("Sounds/coin");

            gameObject.GetComponent<Collider>().isTrigger = true;

            this.steamAudioSource.occlusion = false;
        }

        public override void OnTrigger()
        {
            base.OnTrigger();
            this.audioSource.Stop();
        }
    }
}