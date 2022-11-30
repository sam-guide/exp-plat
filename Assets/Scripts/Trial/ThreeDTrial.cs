using System;
using Config;
using UnityEngine;

namespace Trial
{
    public class ThreeDTrial : TimeoutableTrial
    {
        public ThreeDTrial(int blockId, int trialId) : base(blockId, trialId)
        {
        }

        public override void PreEntry(TrialProgress t, bool first = true)
        {
            base.PreEntry(t, first);
            t.TimingVerification = Config.DataSingleton.GetData().TimingVerification; // timing diagnostics

            // _runningTime -= trialData.Rotate;
            LoadNextSceneWithTimer(trialData.Scene);
        }

        public override void Progress()
        {
            TrialProgress.Num3D++;

            // If we are progressing without a success, record the failure as a zero, otherwise record a 1.
            if (isSuccessful) {TrialProgress.successes.Add(1);}
            else {TrialProgress.successes.Add(0);}

            isSuccessful = false;
            base.Progress();
        }

        public override void Notify()
        {
            // Record that this particular Trial was a success
            TrialProgress.NumSuccess++;
            isSuccessful = true;

        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            var trialEndKeyCode = trialData.TrialEndKey;

            if (!String.IsNullOrEmpty(trialEndKeyCode))
            {
                trialEndKeyCode = trialData.TrialEndKey.ToLower();
            }

            if (Input.GetKey(trialEndKeyCode) && (_runningTime > DataSingleton.GetData().IgnoreUserInputDelay))
            {
                // Debug.Log(_runningTime);
                Progress();
            }
        }
    }
}