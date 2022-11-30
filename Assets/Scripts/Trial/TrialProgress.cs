using System.Collections.Generic;
using Main;
using Controllers.Beacons;
using UnityEngine;

namespace Trial
{
    // The is a data container that pumps data forward through the trials.
    public class TrialProgress
    {
        public AbstractTrial PreviousTrial;
        public float TimeSinceExperimentStart;
        public float NumSuccess;
        public float Num3D;
        public List<int> successes; // Whether a Trial was a success or not (1 or 0).
        public int[] NumCollectedPerBlock; // Number of goals during each block.
        public int TrialNumber;
        public int EnvironmentType;
        public bool TimingVerification; // timing diagnostics boolean
        public int CurrentEnclosureIndex;
        public int PickupType;
        public float targetX, targetY, targetZ;
        public int BlockID;
        public int TrialID;
        public string nom, prenom, age, sex;
        public int instructional;
        public ValidationType validationType;
        public BlockType blockType;
        public TaskType taskType;
        public bool hasValidated;
        public float startPosX, startPosY, startPosZ;
        public float lastPosX, lastPosY, lastPosZ;
        public float lastRotX, lastRotY, lastRotZ;
        public BeaconController targetBeacon;

        public bool isLoaded = true;

        public TrialProgress()
        {
            TrialNumber = -1;
            Num3D = 0;
        }

        public static AbstractTrial GetCurrTrial()
        {
            return Loader.Get().CurrTrial;
        }

        public void ResetOngoing()
        {
            NumSuccess = 0;
            Num3D = 0;
            TrialNumber = -1;
        }

        public void SpecialReset()
        {
            NumSuccess = 0;
            Num3D = 0;
        }
    }
}