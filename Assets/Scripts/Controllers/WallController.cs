using UnityEngine;

namespace Controllers {
    
    public class WallController : MonoBehaviour
    {
        private Camera camera;
        private GameObject mobileAS;
        public AudioClip triggerSound;

        void Awake()
        {
            if (gameObject.GetComponent<Collider>() == null) gameObject.AddComponent<BoxCollider>();

            triggerSound ??= Resources.Load<AudioClip>("Sounds/Metal Impact");

            mobileAS = new GameObject("Wall Audio Source");
            mobileAS.transform.position = gameObject.transform.position;
            mobileAS.transform.SetParent(gameObject.transform);
            mobileAS.AddComponent<WallASController>();

            camera = Camera.main;
        }

        void Start()
        {
            MoveWallASWithPlayer();
        }

        void Update()
        {
            MoveWallASWithPlayer();
        }

        private void MoveWallASWithPlayer()
        {
            if (Vector3.Distance(gameObject.GetComponent<Collider>().ClosestPoint(camera.transform.position), camera.transform.position) <= mobileAS.GetComponent<AudioSource>().maxDistance)
            {
                mobileAS.transform.position = gameObject.GetComponent<Collider>().ClosestPoint(camera.transform.position);
                Debug.DrawLine(camera.transform.position, mobileAS.transform.position, Color.red);
            }
        }
    }
}