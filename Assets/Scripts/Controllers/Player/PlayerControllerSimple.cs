using UnityEngine;
using Controllers.Beacons;

namespace Controllers {
    
    public class PlayerControllerSimple : MonoBehaviour
    {
        private CharacterController characterController;
        private AudioSource playerAudioSource;
        public bool stopBeaconOnPickup = false;
        public float speed = 20f;
        public float mouseSensitivity = 2f;
        public bool _2DLock = true;

        void Awake()
        {
            playerAudioSource = gameObject.GetComponent<AudioSource>();
            characterController = gameObject.GetComponent<CharacterController>();

            gameObject.tag = "Player";
        }

        void Start()
        {
            
        }
    
        void Update()
        {
            Move();
            Rotate();
            
            if(Input.GetKeyDown("space"))
            {
                if(!_2DLock)
                {
                    transform.position = new Vector3(transform.position.x, 3.71f, transform.position.z);
                    transform.rotation = new Quaternion(0, transform.rotation.y, transform.rotation.z, transform.rotation.w);
                }
                _2DLock = !_2DLock;
            }
            if(Input.GetKeyDown("escape")) Application.Quit();
        }
    
        public void Rotate()
        {
            float horizontalRotation = Input.GetAxis("Mouse X");
            float verticalRotation = Input.GetAxis("Mouse Y") * -1f;

            if (_2DLock)
                transform.Rotate(0, horizontalRotation * mouseSensitivity, 0);
            else
            {
                transform.Rotate(verticalRotation * mouseSensitivity, horizontalRotation * mouseSensitivity, 0);
                transform.rotation = new Quaternion(transform.rotation.x, transform.rotation.y, 0, transform.rotation.w);
            }
        }
    
        private void Move()
        {
            float horizontalMove = Input.GetAxis("Horizontal");
            float verticalMove = Input.GetAxis("Vertical");
    
            float verticalSpeed = 0;
            if (_2DLock) verticalSpeed -= 9.81f * Time.deltaTime;
            Vector3 gravityMove = new Vector3(0, verticalSpeed, 0);
    
            Vector3 move = transform.forward * verticalMove + transform.right * horizontalMove;
            characterController.Move(speed * Time.deltaTime * move + gravityMove * Time.deltaTime);
        }

        void OnTriggerEnter(Collider other)
        {
            Debug.Log(gameObject.GetComponent<Collider>().tag + " triggered " + other.GetComponent<Collider>().tag);
            if (other.gameObject.CompareTag("Wall")) playerAudioSource.PlayOneShot(other.GetComponent<WallController>().triggerSound, 0.2f);
            if (other.gameObject.CompareTag("Beacon") || other.gameObject.CompareTag("Pickup")) {
                playerAudioSource.PlayOneShot(other.GetComponent<BeaconController>().triggerSound, 0.6f);
                if (stopBeaconOnPickup) other.GetComponent<BeaconController>().OnTrigger();
            }
        }
    }
}