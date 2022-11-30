using UnityEngine;

namespace Trial
{
    public abstract class TimeoutableTrial : AbstractTrial
    {
        private readonly float _threshHold;

        protected TimeoutableTrial(int blockId, int trialId) : base(blockId, trialId)
        {
            _threshHold = trialData.TrialTime < 1 ? int.MaxValue : trialData.TrialTime;
        }

        //Code for a Trial to continue
        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (_runningTime > _threshHold)
            {
                Debug.Log(_runningTime);
                Progress();
            }
        }
    }
}