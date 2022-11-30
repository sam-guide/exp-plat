using UnityEngine;
using System;
using static Utils.Spatial;

namespace Substitution
{
    public enum FeedbackModality {Audio, Tactile};
    public enum AudioFeedbackType {Clip, SubstitutionManual, SubstitutionPD};
    public enum DistanceEncoding {Intensity, IBI, None};
    public enum AngleEncoding {Frequency, Stereo, None};

    public abstract class SubstitutionSource: MonoBehaviour
    {
        protected Camera player;

        [ReadOnly] public float distanceToPlayer;
        [ReadOnly] public float angleToPlayer;

        protected virtual void Awake()
        {
            player = Camera.main;
        }

        protected virtual void Start()
        {
            distanceToPlayer = GetDistanceToPlayer(player, gameObject);
            angleToPlayer = GetAngleToPlayer2D(player, gameObject);
        }

        protected virtual void Update()
        {
            distanceToPlayer = GetDistanceToPlayer(player, gameObject);
            angleToPlayer = GetAngleToPlayer2D(player, gameObject);
        }

        // public abstract void UpdateBreaks();
        public abstract DistanceEncoding GetDistanceEncoding();
        public abstract AngleEncoding GetAngleEncoding();
    }
}