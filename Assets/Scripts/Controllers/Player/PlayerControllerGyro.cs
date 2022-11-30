using System;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers {
    
    public class PlayerControllerGyro : MonoBehaviour
    {
        /** Improvement: see https://www.youtube.com/watch?v=jvwX5WthM2o **/
        
        public CharacterController characterController;
        public bool stopBeaconOnPickup = false;
        private AudioSource playerAudioSource;
        private Gyroscope gyro;
        public Quaternion rotation;
        private Quaternion initialRotation;
        public Vector3 acceleration;
  
        void Start()
        {
            playerAudioSource = gameObject.GetComponent<AudioSource>();

            initialRotation = transform.rotation;

            if (SystemInfo.supportsGyroscope)
            {
                gyro = Input.gyro;
                gyro.enabled = true;
                rotation = gyro.attitude;
                acceleration = Input.acceleration;
            }
            else
            {
                Debug.LogError("Gyro not initialized properly !");
            }
        }
    
        void Update()
        {
            if (SystemInfo.supportsGyroscope)
            {
                rotation = gyro.attitude;
                acceleration = Input.acceleration;
                Move();
                Rotate();
            }
        }
    
        public void Rotate()
        {
            transform.rotation = Gyro2Unity(rotation);
        }
    
        private void Move()
        {
            float accelX = acceleration.x;
            float accelZ = acceleration.z * -1.0f;
            Vector3 gravityMove = new Vector3(0, 0, accelZ);
    
            Vector3 move = transform.forward * accelZ + transform.right * accelX;
            characterController.Move(7.0f * Time.deltaTime * move + gravityMove * Time.deltaTime);
        }

        private Quaternion Gyro2Unity(Quaternion mobile) {
            return new Quaternion(0, mobile.y, 0, -mobile.w); // TODO: calibrate using initialRotation.y
        }

        void OnTriggerEnter(Collider other) {
            // Debug.Log(gameObject.GetComponent<Collider>().tag + " triggered " + other.GetComponent<Collider>().tag);
            if(other.gameObject.CompareTag("Wall")) playerAudioSource.PlayOneShot(other.GetComponent<WallController>().triggerSound, 0.2f);
        }
    }
}