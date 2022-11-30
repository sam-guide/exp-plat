using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

using static Utils.Spatial;
using Config;
using WallSystem;
using Trial;
using DS = Config.DataSingleton;
using E = Main.Loader;
using Random = UnityEngine.Random;
using Controllers.Beacons;

namespace Controllers
{
    public enum MotionController {KM, G4};

    /**
    *   TODO:
    *   - PlayerControllerKM & PlayerControllerG4 -> inherit from PlayerController
    **/
    public class PlayerController : MonoBehaviour
    {
        public Camera Cam;
        private GenerateGenerateWall _gen;
        private readonly string _outDir; // The stream writer that writes data out to an output file
        private CharacterController _controller; // This is the character controller system used for collision
        private Vector3 _moveDirection = Vector3.zero; // The initial move direction is static zero
        private float startRotationY;
        private bool isTrialEnded = false;
        private bool isTrialStarted = false;
        private bool _reset;
        private int localQuota;

        private AudioClip validationSound;
        private AudioClip moveToNextTrialSound;

        private MotionController motionController;

        /** G4 fields **/
        private PlStream plstream;

        private Vector3 initialPosition, initialG4Position;
        private List<Vector3> positionsWindow = new List<Vector3>();
        private Vector3 currentPosition;

        private Vector4 initialG4Rotation;
        private List<Vector4> rotationsWindow = new List<Vector4>();
        private Quaternion currentRotation;

        /********************************************************/
        #region MonoBehavior methods

        void Awake()
        {
            gameObject.tag = "Player";

            motionController = (MotionController)Enum.Parse(typeof(MotionController), DS.GetData().MotionController ?? "KM", true);
            if (motionController == MotionController.G4 && E.Get().CurrTrial.trialData.Instructional != 1)
            {
                // Debug.Log("!!! Creating PlStream in " + E.Get().CurrTrial.trialData.Instructional);
                plstream = GetComponent<PlStream>() == null ? gameObject.AddComponent<PlStream>() : GetComponent<PlStream>();
            }

            validationSound = Resources.Load<AudioClip>("Sounds/ValidateAnswer");
            moveToNextTrialSound = Resources.Load<AudioClip>("Sounds/NextTrial");
        }

        
        void Start()
        {
            try
            {
                GameObject.Find("TrialText").GetComponent<Text>().text = E.Get().CurrTrial.trialData.TrialInfoText;
                GameObject.Find("BlockText").GetComponent<Text>().text = DS.GetData().Blocks[E.Get().CurrTrial.BlockID].BlockInfoText;

                if (!string.IsNullOrEmpty(E.Get().CurrTrial.trialData.DisplayImage)) {
                    var displayImage = GameObject.Find("DisplayImage").GetComponent<RawImage>();
                    displayImage.enabled = true;
                    displayImage.texture = Img2Sprite.LoadTexture(DS.GetData().SpritesPath + E.Get().CurrTrial.trialData.DisplayImage);
                }

            }
            catch (NullReferenceException e)
            {
                // Debug.LogWarning("Goal object not set: running an instructional Trial");
            }

            Random.InitState(DateTime.Now.Millisecond);

            try
            {
                _controller = GetComponent<CharacterController>();
                _gen = GameObject.Find("WallCreator").GetComponent<GenerateGenerateWall>();

                if (motionController == MotionController.G4 && plstream.active[0] && E.Get().CurrTrial.trialData.Instructional != 1)
                {
                    initialPosition = transform.position;
                    initialG4Position = plstream.positions[0];
                    initialG4Rotation = plstream.orientations[0];
                }
            }
            catch (NullReferenceException e)
            {
                // Debug.LogWarning("Can't set controller object: running an instructional Trial");
            }
            
            _reset = false;
            localQuota = E.Get().CurrTrial.trialData.Quota;

            /* (???) This has to happen here for output to be aligned properly */
            TrialProgress.GetCurrTrial().TrialProgress.TrialNumber++;
            TrialProgress.GetCurrTrial().TrialProgress.blockType = (BlockType)Enum.Parse(typeof(BlockType), DS.GetData().Blocks[TrialProgress.GetCurrTrial().BlockID].BlockType ?? "TwoD", true);
            TrialProgress.GetCurrTrial().TrialProgress.taskType = (TaskType)Enum.Parse(typeof(TaskType), DS.GetData().Blocks[TrialProgress.GetCurrTrial().BlockID].TaskType ?? "Navigation", true);
            TrialProgress.GetCurrTrial().TrialProgress.validationType = (ValidationType)Enum.Parse(typeof(ValidationType), DS.GetData().Blocks[TrialProgress.GetCurrTrial().BlockID].ValidationType ?? "Collision", true);
            TrialProgress.GetCurrTrial().TrialProgress.instructional = TrialProgress.GetCurrTrial().trialData.Instructional;
            TrialProgress.GetCurrTrial().TrialProgress.EnvironmentType = TrialProgress.GetCurrTrial().trialData.Scene;
            TrialProgress.GetCurrTrial().TrialProgress.CurrentEnclosureIndex = TrialProgress.GetCurrTrial().trialData.Enclosure - 1;
            TrialProgress.GetCurrTrial().TrialProgress.BlockID = TrialProgress.GetCurrTrial().BlockID;
            TrialProgress.GetCurrTrial().TrialProgress.TrialID = TrialProgress.GetCurrTrial().TrialID;
            TrialProgress.GetCurrTrial().TrialProgress.targetX = 0;
            TrialProgress.GetCurrTrial().TrialProgress.targetY = 0;
            TrialProgress.GetCurrTrial().TrialProgress.targetZ = 0;
            TrialProgress.GetCurrTrial().TrialProgress.hasValidated = false;

            isTrialStarted = true;
        }


        void Update()
        {
            // [MAR] This reset TrialTime once when Update() loop starts (???)
            if (!_reset)
            {
                _reset = true;
                TrialProgress.GetCurrTrial().ResetTime();
            }

            if (TrialProgress.GetCurrTrial().TrialProgress.instructional != 1)
            {
                E.LogData(TrialProgress.GetCurrTrial().TrialProgress, TrialProgress.GetCurrTrial().TrialStartTime, transform);
            }

            // Wait for the sound to finish playing before ending the Trial
            if (isTrialEnded && !GetComponent<AudioSource>().isPlaying)
            {
                // [MAR]: (TODO) Move logic to Abstract or 3D Trial ?
                bool isPressToProgress = Convert.ToBoolean(DS.GetData().Blocks[TrialProgress.GetCurrTrial().BlockID].IsPressToProgress);

                if (isPressToProgress)
                {
                    if (Input.GetKeyDown(KeyCode.PageUp) || Input.GetKeyDown(KeyCode.PageDown) || Input.GetKeyDown(KeyCode.Space)) 
                    {
                        GetComponent<AudioSource>().PlayOneShot(moveToNextTrialSound, 1.0f);
                        Thread.Sleep(500);
                        MoveToNextTrial();
                    }
                }
                else MoveToNextTrial();
            }

            // Enact signal masking if required
            if (TrialProgress.GetCurrTrial().TrialProgress.instructional != 1 && 
                isTrialStarted && 
                Convert.ToBoolean(DS.GetData().Blocks[TrialProgress.GetCurrTrial().BlockID].IsMasked))
            {
                if (!isTrialEnded && 
                    Convert.ToBoolean(DS.GetData().Blocks[TrialProgress.GetCurrTrial().BlockID].IsMasked) && 
                    HasMetMaskingConditions()) 
                {
                    TrialProgress.GetCurrTrial().TrialProgress.targetBeacon.audioSource.Stop();
                }
            }

            // Move the character (unless the Quota has been reached)
            // [MAR]: (TODO) Should we keep the Quota mechanic ?
            if ((localQuota > 0 | E.Get().CurrTrial.trialData.Quota == 0) && E.Get().CurrTrial.trialData.Instructional != 1)
            {
                try
                {
                    // Watch for manual validation when it applies
                    if (!TrialProgress.GetCurrTrial().TrialProgress.hasValidated && 
                        TrialProgress.GetCurrTrial().TrialProgress.validationType != ValidationType.Collision)
                    {
                        if (Input.GetKeyDown(KeyCode.PageUp) || Input.GetKeyDown(KeyCode.PageDown) || Input.GetKeyDown(KeyCode.Space)) 
                            TriggerManualValidation();
                    }
                    // Watch for movements & rotation commands
                    Move();
                    Rotate();
                    
                    if (Input.GetKeyDown("escape")) Application.Quit();
                }
                catch (MissingComponentException e)
                {
                    // Debug.LogWarning("Skipping movement calc: instructional Trial");
                }
            }
        }


        // Automatic validation of the trial on collision
        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Pickup"))
            {
                // If the current Trial should be validated automatically on Collision
                if (TrialProgress.GetCurrTrial().TrialProgress.validationType == ValidationType.Collision) 
                {
                    GameObject target = TrialProgress.GetCurrTrial().TrialProgress.targetBeacon.gameObject;
                    GetComponent<AudioSource>().PlayOneShot(target.GetComponent<BeaconController>().triggerSound, 1.0f);
                    
                    ValidateCurrentTrial();
                    EndCurrentTrial();
                }

                // If the validation is Manual with feedback after (until Collision)
                if (TrialProgress.GetCurrTrial().TrialProgress.validationType == ValidationType.ManualPlusCollision)
                {
                    // If the user has already validated his answer
                    if (TrialProgress.GetCurrTrial().TrialProgress.hasValidated) {
                        GameObject target = TrialProgress.GetCurrTrial().TrialProgress.targetBeacon.gameObject;
                        GetComponent<AudioSource>().PlayOneShot(target.GetComponent<BeaconController>().triggerSound, 1.0f);

                        EndCurrentTrial();
                    }
                }
            }
            
            if (other.gameObject.CompareTag("HomingStart"))
            {
                TrialProgress.GetCurrTrial().TrialProgress.targetBeacon.audioSource.Stop();
            }
        }

        // Automatic validation of the trial on collision (Stay)
        void OnTriggerStay(Collider other)
        {
            if (!other.gameObject.CompareTag("Pickup")) return;
            
            // If the validation is Manual with feedback after (until Collision)
            if (TrialProgress.GetCurrTrial().TrialProgress.validationType == ValidationType.ManualPlusCollision)
            {
                // If the user has already validated his answer
                if (TrialProgress.GetCurrTrial().TrialProgress.hasValidated) {
                    GameObject target = TrialProgress.GetCurrTrial().TrialProgress.targetBeacon.gameObject;
                    GetComponent<AudioSource>().PlayOneShot(target.GetComponent<BeaconController>().triggerSound, 1.0f);
                    Thread.Sleep(250);
                    EndCurrentTrial();
                }
            }
        }

        #endregion
        /********************************************************/



        // Start the character. If init from enclosure, this allows "s" to determine the start position
        public void ExternalStart(float pickX, float pickY, float pickZ, bool useEnclosure = false)
        {
            while (!isTrialStarted) Thread.Sleep(20);

            /** Fill in new TrialProgress **/
            TrialProgress.GetCurrTrial().TrialProgress.targetX = pickX;
            TrialProgress.GetCurrTrial().TrialProgress.targetY = pickY;
            TrialProgress.GetCurrTrial().TrialProgress.targetZ = pickZ;

            // First (non-instructional) trial of first block
            // [MAR] TODO: depends on the number of instruction trials in the block --> needs better indexing
            if (E.Get().CurrTrial.TrialID == 1 && E.Get().CurrTrial.BlockID == 0)
            {
                /** Rotation **/
                if (E.Get().CurrTrial.trialData.StartFacing == -1) startRotationY = Random.Range(0, 360);
                // [MAR] TODO: else if (isMotionCapture) startRotationY = MC.GetY();
                else startRotationY = E.Get().CurrTrial.trialData.StartFacing;
                transform.Rotate(0, startRotationY, 0);

                // If no position specified --> Try to randomly place the character, checking for proximity to the Target location
                if (E.Get().CurrTrial.trialData.StartPosition.Count == 0)
                {
                    // [MAR] Do we really need this ? (it's never needed if using motion capture --> the player's position is never unknown)
                    // [MAR] TODO: rework with some kind of optimization (instead of a random search ...)
                    int i = 0;
                    while (i++ < 300) 
                    {
                        Vector2 rngPos = RandomPositionInRadius(DS.GetData().Enclosures[E.Get().CurrTrial.TrialProgress.CurrentEnclosureIndex].Radius);
                        float distanceToTarget = Vector3.Distance(rngPos, new Vector2(pickX, pickZ)); // [MAR] TODO: use Utils.Spatial
                        if (distanceToTarget > DS.GetData().CharacterData.DistancePickup)
                        {
                            transform.position = new Vector3(rngPos.x, 0.5f, rngPos.y);
                            var camPos = Cam.transform.position;
                            camPos.y = DS.GetData().CharacterData.Height;
                            Cam.transform.position = camPos;

                            TrialProgress.GetCurrTrial().TrialProgress.startPosX = transform.position.x;
                            TrialProgress.GetCurrTrial().TrialProgress.startPosY = transform.position.y;
                            TrialProgress.GetCurrTrial().TrialProgress.startPosZ = transform.position.z;

                            return;
                        }
                    }
                    Debug.LogError("Could not randomly place player. Probably due to a pick up location setting");
                }
                // Starting position is manually specified
                else
                {
                    var p = E.Get().CurrTrial.trialData.StartPosition;                    

                    if (useEnclosure) p = new List<float>() { pickX, pickZ };

                    transform.position = new Vector3(p[0], 0.5f, p[1]);
                    Cam.transform.position = new Vector3(Cam.transform.position.x, DS.GetData().CharacterData.Height, Cam.transform.position.z);
                }
            }
            // Any trial after the first
            else
            {
                transform.position = new Vector3(
                    E.Get().CurrTrial.TrialProgress.lastPosX, 
                    E.Get().CurrTrial.TrialProgress.lastPosY, 
                    E.Get().CurrTrial.TrialProgress.lastPosZ
                );
                transform.Rotate(0, E.Get().CurrTrial.TrialProgress.lastRotY, 0);

                TrialProgress.GetCurrTrial().TrialProgress.startPosX = transform.position.x;
                TrialProgress.GetCurrTrial().TrialProgress.startPosY = transform.position.y;
                TrialProgress.GetCurrTrial().TrialProgress.startPosZ = transform.position.z;
            }

            if (TrialProgress.GetCurrTrial().TrialProgress.taskType == TaskType.Homing)
            {
                List<float> spawnDistances = DS.GetData().Blocks[TrialProgress.GetCurrTrial().BlockID].SpawnDistanceToPlayer;
                float rngRadiusInSpawnDistances = 0f;

                GameObject prefab = (GameObject)Resources.Load("3D_Objects/Sphere", typeof(GameObject));
                GameObject obj = Instantiate(prefab);
                obj.GetComponent<Collider>().isTrigger = true;
                obj.tag = "HomingStart";

                BoxCollider roomCollider = GameObject.FindGameObjectsWithTag("Room")[0].GetComponent<BoxCollider>();
                Vector3 pos = new Vector3(0f, 0.5f, 0f);

                int j = 0;
                while (j++ < 300)
                {
                    if (spawnDistances.Count == 1) rngRadiusInSpawnDistances = Random.Range(0, spawnDistances[1]);
                    else if (spawnDistances.Count == 2) rngRadiusInSpawnDistances = Random.Range(spawnDistances[0], spawnDistances[1]);
                    else if (spawnDistances.Count > 2) rngRadiusInSpawnDistances = spawnDistances[new System.Random().Next(spawnDistances.Count)];
                    
                    Vector3 homingStartPos = (transform.rotation * Vector3.forward * rngRadiusInSpawnDistances) + transform.position;
                    pos = new Vector3(homingStartPos.x, 2, homingStartPos.z);

                    if (roomCollider.bounds.Contains(pos))
                    {
                        Debug.Log("[Homing] Rng In SpawnDistances: " + rngRadiusInSpawnDistances);
                        obj.transform.position = pos;
                        break;
                    }
                }

                if (obj.transform.position == new Vector3(0f, 0.5f, 0f))
                {
                    obj.transform.position = pos;
                    Debug.LogWarning("Homing intermediate target had to be placed outside the room !!!");
                }
            }
        }

        /** Movements **/
        private void Move()
        {
            switch(motionController)
            {
                case MotionController.KM:
                    MoveKM();
                    break;
                case MotionController.G4:
                    if (plstream.active[0]) MoveG4();
                    break;
            }
        }

        private void MoveKM()
        {
            float horizontalMove = Input.GetAxis("Horizontal");
            float verticalMove = Input.GetAxis("Vertical");
            Vector3 move = transform.forward * verticalMove + transform.right * horizontalMove;
    
            // Effect of Gravity
            float verticalSpeed = 0;
            if (!_controller.isGrounded) verticalSpeed -= 9.81f * Time.deltaTime;
            Vector3 gravityMove = new Vector3(0, verticalSpeed, 0);
    
            // Combining movement with gravity
            _controller.Move(DS.GetData().CharacterData.MovementSpeed * Time.deltaTime * move + gravityMove * Time.deltaTime);
        }

        private void MoveG4()
        {
            if (positionsWindow.Count < 20) positionsWindow.Add(plstream.positions[0]);
            else
            {
                Vector3 averagedG4Position = positionsWindow.Aggregate(new Vector3(0,0,0), (s,v) => s + v) / (float)positionsWindow.Count;
                // positionsWindow.Clear();
                positionsWindow.RemoveAt(0);

                currentPosition = G4Pos2Unity(averagedG4Position) - G4Pos2Unity(initialG4Position) + initialPosition;
                currentPosition.y = DS.GetData().CharacterData.Height;

                transform.position = currentPosition;
            }
        }

        /** Rotations **/
        public void Rotate()
        {
            switch(motionController)
            {
                case MotionController.KM:
                    RotateKM();
                    break;
                case MotionController.G4:
                    if (plstream.active[0]) RotateG4();
                    break;
            }
        }

        private void RotateKM()
        {
            float horizontalRotation = Input.GetAxis("Mouse X") * DS.GetData().CharacterData.RotationSpeed * Time.deltaTime;    
            transform.Rotate(0, horizontalRotation, 0);
        }

        private void RotateG4()
        {
            if (rotationsWindow.Count < 20) rotationsWindow.Add(plstream.orientations[0]);
            else
            {
                Vector4 averagedG4Rotation = rotationsWindow.Aggregate(new Vector4(0,0,0,0), (s,v) => s + v) / (float)rotationsWindow.Count;
                // rotationsWindow.Clear();
                rotationsWindow.RemoveAt(0);

                currentRotation = Quaternion.Euler(V42Quat(averagedG4Rotation).eulerAngles - V42Quat(initialG4Rotation).eulerAngles);
                currentRotation.x = 0;
                currentRotation.z = 0;

                transform.rotation = currentRotation;
            }
        }

        private Vector3 G4Pos2Unity(Vector3 g4_pos)
        {
            return(new Vector3(g4_pos.y, g4_pos.z, -g4_pos.x));
        }

        private Quaternion V42Quat(Vector4 v4)
        {
            return(new Quaternion(v4.x, v4.y, v4.z, v4.w));
        }

        private bool HasMetMaskingConditions()
        {
            Vector3 targetPosition = new Vector3(TrialProgress.GetCurrTrial().TrialProgress.targetX, TrialProgress.GetCurrTrial().TrialProgress.targetY, TrialProgress.GetCurrTrial().TrialProgress.targetZ);
            Vector3 startingPosition = new Vector3(TrialProgress.GetCurrTrial().TrialProgress.startPosX, TrialProgress.GetCurrTrial().TrialProgress.startPosY, TrialProgress.GetCurrTrial().TrialProgress.startPosZ);

            float startingDistance = UnityEngine.Vector3.Distance(targetPosition, startingPosition);
            float currentDistance = UnityEngine.Vector3.Distance(targetPosition, transform.position);
            float distanceCovered = Mathf.Abs(startingDistance - currentDistance);
            float percDistanceCovered = distanceCovered / startingDistance;

            // Debug.Log("Distance covered: " + distanceCovered);
            // Debug.Log("% distance covered: " + percDistanceCovered);

            // Has crossed 80% of the distance to the target
            // if (percDistanceCovered >= 0f && percDistanceCovered < 1f && percDistanceCovered >= 0.8f) return(true);

            if (distanceCovered >= 5) return(true);
            else return(false);
        }


        // Watching for the user pressing the validation key
        public void TriggerManualValidation()
        {            
            TrialProgress.GetCurrTrial().TrialProgress.hasValidated = true;
            GetComponent<AudioSource>().PlayOneShot(validationSound, 1.0f);

            if (TrialProgress.GetCurrTrial().TrialProgress.validationType == ValidationType.Manual) 
            {
                ValidateCurrentTrial();
                EndCurrentTrial();
            }
            // ValidationType == ValidationType.ManualPlusCollision
            else {
                ValidateCurrentTrial();
            }
        }


        private void ValidateCurrentTrial()
        {           
            // Tally the number collected per current block
            // TrialProgress.GetCurrTrial().TrialProgress.NumCollectedPerBlock[TrialProgress.GetCurrTrial().BlockID]++;
            // TrialProgress.GetCurrTrial().NumCollected++;

            // Log the fact that the target was found (or the trial was manually validated)
            E.LogData(TrialProgress.GetCurrTrial().TrialProgress, TrialProgress.GetCurrTrial().TrialStartTime, transform, 1);
        }


        private void EndCurrentTrial()
        {
            Destroy(TrialProgress.GetCurrTrial().TrialProgress.targetBeacon.gameObject);

            isTrialEnded = true;
        }


        private void MoveToNextTrial()
        {
            E.Get().CurrTrial.Notify();

            // Stops movement after hitting target until localQuota has been reset
            // if (--localQuota > 0) return;

            /* Log the final position & rotation */
            E.Get().CurrTrial.TrialProgress.lastPosX = transform.position.x;
            E.Get().CurrTrial.TrialProgress.lastPosY = transform.position.y;
            E.Get().CurrTrial.TrialProgress.lastPosZ = transform.position.z;

            E.Get().CurrTrial.TrialProgress.lastRotX = transform.eulerAngles.x;
            E.Get().CurrTrial.TrialProgress.lastRotY = transform.eulerAngles.y;
            E.Get().CurrTrial.TrialProgress.lastRotZ = transform.eulerAngles.z;

            TrialProgress.GetCurrTrial().Progress();
            
            isTrialEnded = false;
        }
    }
}
