using System;
using System.Collections.Generic;
// using System.Diagnostics;
using UnityEngine;
using Utils;
using static Utils.Spatial;
using BlockType = Trial.BlockType;
using Config;
using Controllers;
using Controllers.Beacons;
using Substitution;
using DS = Config.DataSingleton;
using E = Main.Loader;
using Random = UnityEngine.Random;

namespace WallSystem
{
    public class PickupGenerator : MonoBehaviour
    {
        private List<GameObject> _destroy;

        private void Start()
        {
            _destroy = new List<GameObject>(); // This initializes the pickup destroy list

            List<int> beacons = E.Get().CurrTrial.trialData.BeaconList;
            BlockType blockType = E.Get().CurrTrial.TrialProgress.blockType;

            Data.Point p = new Data.Point { X = 0, Y = 0, Z = 0 };

            BoxCollider roomCollider = GameObject.FindGameObjectsWithTag("Room")[0].GetComponent<BoxCollider>();

            foreach (int beacon in beacons) {
                var b = DS.GetData().Beacons[Mathf.Abs(beacon) - 1];

                /** Computing the beacon's position **/

                List<float> spawnDistances = DS.GetData().Blocks[E.Get().CurrTrial.BlockID].SpawnDistanceToPlayer;
                float rngRadiusInSpawnDistances = 0f;

                List<float> spawnAngles = DS.GetData().Blocks[E.Get().CurrTrial.BlockID].SpawnAngleToPlayer;
                float rngRotYInSpawnAngles = 0f;

                Quaternion playerRot = Quaternion.identity;
                Vector3 playerPos = new Vector3(0, 0, 0);

                // First (non-instructional) trial of first block
                if (E.Get().CurrTrial.TrialID == 1 && E.Get().CurrTrial.BlockID == 0)
                {
                    Debug.Log("[2D] First trial from the first block");

                    // [MAR] TODO:
                    // if (isMC) take position from MC
                    // else

                    List<float> posFromJSON = new List<float>() {0f, 0.5f, 0f}; // [MAR] TODO: if (use enclosure) --> get position (from Data)
                    if (E.Get().CurrTrial.trialData.StartPosition.Count == 3)
                        posFromJSON = E.Get().CurrTrial.trialData.StartPosition;

                    playerPos = new Vector3(posFromJSON[0], posFromJSON[1], posFromJSON[2]);
                    playerRot.eulerAngles = new Vector3(0, E.Get().CurrTrial.trialData.StartFacing, 0);
                }
                else
                {
                    playerPos = new Vector3(
                        E.Get().CurrTrial.TrialProgress.lastPosX, 
                        E.Get().CurrTrial.TrialProgress.lastPosY, 
                        E.Get().CurrTrial.TrialProgress.lastPosZ
                    );
                
                    playerRot.eulerAngles = new Vector3(
                        E.Get().CurrTrial.TrialProgress.lastRotX, 
                        E.Get().CurrTrial.TrialProgress.lastRotY, 
                        E.Get().CurrTrial.TrialProgress.lastRotZ
                    );
                }

                /** 1D block: only the distance to the player varies **/
                if (blockType == BlockType.OneD)
                {
                    int i = 0;
                    while (i++ < 300) 
                    {
                        if (spawnDistances.Count == 1) rngRadiusInSpawnDistances = Random.Range(0, spawnDistances[1]);
                        else if (spawnDistances.Count == 2) rngRadiusInSpawnDistances = Random.Range(spawnDistances[0], spawnDistances[1]);
                        else if (spawnDistances.Count > 2) rngRadiusInSpawnDistances = spawnDistances[new System.Random().Next(spawnDistances.Count)];
                        
                        Vector3 pos = (playerRot * Vector3.forward * rngRadiusInSpawnDistances) + playerPos;

                        if (roomCollider.bounds.Contains(pos))
                        {
                            p = new Data.Point { X = pos.x, Y = 0.5f, Z = pos.z };

                            // Debug.Log("[1D] Rng In SpawnDistances: " + rngRadiusInSpawnDistances);
                            break;
                        }
                    }
                    if (!roomCollider.bounds.Contains(new Vector3(p.X, p.Y, p.Z)))
                    {
                        Debug.LogError("Could not find a homing intermediate target position inside the room !!!");
                    }
                }

                /** 2D Block: both the distance and horizontal angle from the player vary **/
                else if (blockType == BlockType.TwoD)
                {
                    // No spawn position is provided in the JSON
                    if (b.PositionVector.x == 0 && b.PositionVector.y == 0 && b.PositionVector.z == 0)
                    {
                        // [MAR] TODO: add case when contraint on one but not the other
                        // No contraints in distance or angle from player are provided: fully randomized
                        if (spawnAngles.Count == 0 && spawnDistances.Count == 0)
                        {
                            int j = 0;
                            while (j++ < 300) 
                            {
                                Vector2 pos = RandomPositionInRadius(DS.GetData().Enclosures[E.Get().CurrTrial.TrialProgress.CurrentEnclosureIndex].Radius);
                                
                                if (roomCollider.bounds.Contains(pos))
                                {
                                    p = new Data.Point { X = pos.x, Y = 0.5f, Z = pos.y };
                                    break;
                                }
                            }
                            if (!roomCollider.bounds.Contains(new Vector3(p.X, p.Y, p.Z)))
                            {
                                Debug.LogError("Could not find a spawn position for the target inside the room !!!");
                            }
                        }

                        int k = 0;
                        while (k++ < 300) 
                        {
                            // Computing spawn distance (from player)
                            if (spawnDistances.Count == 1) rngRadiusInSpawnDistances = Random.Range(0, spawnDistances[1]);
                            else if (spawnDistances.Count == 2) rngRadiusInSpawnDistances = Random.Range(spawnDistances[0], spawnDistances[1]);
                            else if (spawnDistances.Count > 2) rngRadiusInSpawnDistances = spawnDistances[new System.Random().Next(spawnDistances.Count)];
                            // Debug.Log("[2D] Rng In SpawnDistances: " + rngRadiusInSpawnDistances);

                            // Computing spawn angle (from player)
                            if (spawnAngles.Count == 1) rngRotYInSpawnAngles = Random.Range(0, spawnAngles[1]);
                            else if (spawnAngles.Count == 2) rngRotYInSpawnAngles = Random.Range(spawnAngles[0], spawnAngles[1]);
                            else if (spawnAngles.Count > 2) rngRotYInSpawnAngles = spawnAngles[new System.Random().Next(spawnAngles.Count)];
                            // Debug.Log("[2D] Rng In SpawnAngles: " + rngRotYInSpawnAngles);

                            // Rotate player's position by selected angle to compute the "facing direction" in which to spawn the beacon
                            playerRot *= Quaternion.Euler(0, rngRotYInSpawnAngles, 0);

                            Vector3 pos = (playerRot * Vector3.forward * rngRadiusInSpawnDistances) + playerPos;

                            if (roomCollider.bounds.Contains(pos))
                            {
                                p = new Data.Point { X = pos.x, Y = 0.5f, Z = pos.z };
                                break;
                            }
                        }
                        if (!roomCollider.bounds.Contains(new Vector3(p.X, p.Y, p.Z)))
                        {
                            Debug.LogError("Could not find a spawn position for the target inside the room !!!");
                        }
                    }
                    // Spawn coordinates are provided in the JSON
                    else
                    {
                        try {p = new Data.Point { X = b.PositionVector.x, Y = b.PositionVector.y, Z = b.PositionVector.z };}
                        catch (Exception _) {p = new Data.Point { X = b.PositionVector.x, Y = 0.5f, Z = b.PositionVector.z };}
                    }
                }

                // [MAR]: TODO
                else if (blockType == BlockType.ThreeD)
                {

                }

                GameObject prefab = (GameObject)Resources.Load("3D_Objects/" + b.Object, typeof(GameObject));
                GameObject obj = Instantiate(prefab);

                // Positioning the object
                obj.transform.Rotate(b.RotationVector);
                obj.transform.localScale = b.ScaleVector;
                obj.transform.position = new Vector3(p.X, p.Y, p.Z);


                /** Computing the beacon's type **/
                switch ((BeaconType) Enum.Parse(typeof(BeaconType), b.BeaconType, true))
                {
                    case BeaconType.Target: 
                        obj.AddComponent<TargetBeaconController>();
                        E.Get().CurrTrial.TrialProgress.targetBeacon = obj.GetComponent<BeaconController>();
                        break;
                    case BeaconType.Waypoint: 
                        obj.AddComponent<WaypointBeaconController>();
                        break;
                    case BeaconType.Landmark: 
                        obj.AddComponent<LandmarkBeaconController>();
                        break;
                    case BeaconType.Center: 
                        // obj.AddComponent<CenterBeaconController>();
                        break;
                }

                FeedbackModality fm = (FeedbackModality) Enum.Parse(typeof(FeedbackModality), b.FeedbackModality, true);
                obj.GetComponent<BeaconController>().modality = fm;
                switch (fm)
                {
                    case FeedbackModality.Tactile:
                        obj.AddComponent<TactileSubstitution>();
                        obj.GetComponent<TactileSubstitution>().Init(
                            DS.GetData().MotorOrder,
                            b.DistanceEncoding, b.DistanceMin, b.DistanceMax, 
                            b.DistanceToIntensityLink, b.IntensityMin, b.IntensityMax, b.IntensitySteps,
                            b.DistanceToIBILink, b.IBIMin, b.IBIMax, b.IBISteps,
                            /** b.AngleEncoding, **/ b.AngleMin, b.AngleMax
                        );
                        E.Get().CurrTrial.TrialProgress.targetBeacon.tactileSubstitution = obj.GetComponent<TactileSubstitution>();
                        break;
                    case FeedbackModality.Audio:
                        AudioFeedbackType aft = (AudioFeedbackType) Enum.Parse(typeof(AudioFeedbackType), b.AudioFeedbackType, true);
                        switch (aft)
                        {
                            case AudioFeedbackType.Clip:
                                obj.AddComponent<AudioSubstitutionClip>();
                                obj.GetComponent<AudioSubstitutionClip>().Init(b.AudioClipName);
                                E.Get().CurrTrial.TrialProgress.targetBeacon.audioSubstitution = obj.GetComponent<AudioSubstitutionClip>();
                                break;
                            case AudioFeedbackType.SubstitutionPD:
                                obj.AddComponent<AudioSubstitutionPD>();
                                obj.GetComponent<AudioSubstitutionPD>().Init(
                                    b.DistanceEncoding, b.DistanceMin, b.DistanceMax, 
                                    b.DistanceToIntensityLink, b.IntensityMin, b.IntensityMax, b.IntensitySteps,
                                    b.DistanceToIBILink, b.IBIMin, b.IBIMax, b.IBISteps,
                                    b.AngleEncoding, b.AngleMin, b.AngleMax,
                                    b.AngleToFrequencyLink, b.FrequencyMin, b.FrequencyMax, b.FrequencySteps,
                                    b.PatchName
                                );
                                E.Get().CurrTrial.TrialProgress.targetBeacon.audioSubstitution = obj.GetComponent<AudioSubstitutionPD>();
                                break;
                            case AudioFeedbackType.SubstitutionManual:
                                obj.AddComponent<AudioSubstitutionManual>();
                                obj.GetComponent<AudioSubstitutionManual>().Init(
                                    b.DistanceEncoding, b.DistanceMin, b.DistanceMax, 
                                    b.DistanceToIntensityLink, b.IntensityMin, b.IntensityMax, b.IntensitySteps,
                                    b.DistanceToIBILink, b.IBIMin, b.IBIMax, b.IBISteps,
                                    b.AngleEncoding, b.AngleMin, b.AngleMax,
                                    b.AngleToFrequencyLink, b.FrequencyMin, b.FrequencyMax, b.FrequencySteps
                                );
                                E.Get().CurrTrial.TrialProgress.targetBeacon.audioSubstitution = obj.GetComponent<AudioSubstitutionManual>();
                                break;
                        }
                        obj.GetComponent<AudioSubstitution>().audioFeedbackType = aft;
                        break;
                }

                _destroy.Add(obj);
            }

            GameObject.Find("Participant").GetComponent<PlayerController>().ExternalStart(p.X, p.Y, p.Z);
        }

        //And here we destroy all the food.
        private void OnDestroy()
        {
            foreach (var t in _destroy)
            {
                if (t != null) Destroy(t);
            }
        }
    }
}