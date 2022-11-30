using UnityEngine;

namespace Controllers.Beacons
{
    public class LandmarkBeaconController : BeaconController
    {
        protected override void Awake()
        {
            base.Awake();

            if (this.triggerSound == null) this.triggerSound = Resources.Load<AudioClip>("Sounds/coin"); 
            // this.triggerSound ??= Resources.Load<AudioClip>("Sounds/coin");
            gameObject.GetComponent<Collider>().isTrigger = false;

            this.steamAudioSource.occlusion = false;
        }

        public override void OnTrigger()
        {
            base.OnTrigger();
        }
    }
}