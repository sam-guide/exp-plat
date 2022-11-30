﻿using System;
using Config;
using UnityEngine;

namespace Trial
{
    //We have the ClosingTrial.
    
    //If we are in this state, then we finish the experiment.
    public class CloseTrial : AbstractTrial
    {
        public CloseTrial(int blockId, int trialId) : base(blockId, trialId)
        {

        }

        public override void PreEntry(TrialProgress t, bool first = true)
        {
            base.PreEntry(t, first);
            Debug.Log("In close Trial");
        }

        public override void Progress()
        {
            //This should never be called
        }

    }
}