using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controllers.Beacons;
using Utils;

namespace Controllers {
    
    public class PlayerControllerG4 : MonoBehaviour
    {
        private CharacterController characterController;
        private AudioSource playerAudioSource;
        public bool stopBeaconOnPickup = false;

        private PlStream plstream;

        [ReadOnly] public Vector3 initialPosition, initialG4Position, currentG4Position, previousG4Position;
        private List<Vector3> positionsWindow = new List<Vector3>();
        [ReadOnly] public Vector3 currentPosition;

        private Vector3 initialG4Rotation;
        private List<Quaternion> rotationsWindow = new List<Quaternion>();
        [ReadOnly] public Quaternion currentRotation;
        
        public float defaultHeight = 3f;

        void Awake()
        {
            plstream = GetComponent<PlStream>() == null ? gameObject.AddComponent<PlStream>() : GetComponent<PlStream>();
            playerAudioSource = GetComponent<AudioSource>() == null ? gameObject.AddComponent<AudioSource>() : GetComponent<AudioSource>();
            characterController = GetComponent<CharacterController>();

            gameObject.tag = "Player";
        }

        void Start()
        {
            initialPosition = transform.position;
            initialG4Position = plstream.positions[0];
            currentG4Position = initialG4Position;
            initialG4Rotation = G4Rot2Unity(plstream.orientations[0]).eulerAngles;
        }
    
        void Update()
        {
            if (plstream.active[0])
            {

                /** Position **/
                /* if (positionsWindow.Count < 20) positionsWindow.Add(plstream.positions[0]);
                else
                {
                    Vector3 averagedG4Position = positionsWindow.Aggregate(new Vector3(0,0,0), (s,v) => s + v) / (float)positionsWindow.Count;
                    positionsWindow.RemoveAt(0);

                    currentPosition = G4Pos2Unity(averagedG4Position) - G4Pos2Unity(initialG4Position) + initialPosition;
                    currentPosition.y = defaultHeight;

                    transform.position = currentPosition;
                } */

                /** Orientation **/
                /* if (rotationsWindow.Count < 20) rotationsWindow.Add(G4Rot2Unity(plstream.orientations[0]));
                else
                {
                    Quaternion averagedG4Rotation = Spatial.QuaternionAverage(rotationsWindow);
                    rotationsWindow.RemoveAt(0);

                    currentRotation = Quaternion.Euler(averagedG4Rotation.eulerAngles - initialG4Rotation);
                    currentRotation.x = 0;
                    currentRotation.z = 0;

                    transform.rotation = currentRotation;
                } */

                currentPosition = G4Pos2Unity(plstream.positions[0] - initialG4Position) + initialPosition;
                currentPosition.y = defaultHeight;

                transform.position = currentPosition;

                /* TODO (idée 1)
                - Sensor moving forward (when stuck in the back): -Y --> transform.forward(current.y - previous.y)
                - Sensor moving laterally (//): X --> transform.right(current.x - previous.x)
                **/

                /*
                previousG4Position = currentG4Position;
                currentG4Position = plstream.positions[0];

                Vector3 move = transform.forward * (currentG4Position - previousG4Position).y * -1f + 
                                transform.right * (currentG4Position - previousG4Position).x;

                
                characterController.Move(200f * Time.deltaTime * move);
                */

                currentRotation = Quaternion.Euler(G4Rot2Unity(plstream.orientations[0]).eulerAngles - initialG4Rotation);
                // TODO: make x & z ZERO before Quaternion
                currentRotation.x = 0;
                currentRotation.z = 0;

                transform.rotation = currentRotation;

                /* TODO (idée 2)
                - Rotate movement data to match referential of player
                **/
            }

            if (Input.GetKeyDown("escape")) Application.Quit();
        }

        private Vector3 G4Pos2Unity(Vector3 g4_pos)
        {
            // return(new Vector3(g4_pos.y, g4_pos.z, -g4_pos.x));
            return (new Vector3(g4_pos.y, g4_pos.z, g4_pos.x));
        }

        private Quaternion G4Rot2Unity(Vector4 v4)
        {
            /*
            unity_rotation.w = pol_rotation[0]; // W
            unity_rotation.x = -pol_rotation[2]; // Y
            unity_rotation.y = pol_rotation[3]; // Z
            unity_rotation.z = -pol_rotation[1]; // X
            */
            return (new Quaternion(-v4.y, v4.z, -v4.x, v4.w));
            // return (new Quaternion(v4.x, v4.y, v4.z, v4.w));
        }

        private void OnTriggerEnter(Collider other)
        {
            // Debug.Log(gameObject.GetComponent<Collider>().tag + " triggered " + other.GetComponent<Collider>().tag);
            if (other.gameObject.CompareTag("Wall")) playerAudioSource.PlayOneShot(other.GetComponent<WallController>().triggerSound, 0.2f);
            if (other.gameObject.CompareTag("Beacon") || other.gameObject.CompareTag("Pickup")) {
                playerAudioSource.PlayOneShot(other.GetComponent<BeaconController>().triggerSound, 0.6f);
                if (stopBeaconOnPickup) other.GetComponent<BeaconController>().OnTrigger();
            }
        }
    }
}