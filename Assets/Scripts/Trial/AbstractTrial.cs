using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Main;
using UnityEngine;
using UnityEngine.SceneManagement;
using Config;
using static Config.Data;
using DS = Config.DataSingleton;

namespace Trial
{

    public enum ValidationType {Collision, Manual, ManualPlusCollision};
    public enum BlockType {OneD, TwoD, ThreeD};
    public enum TaskType {Navigation, Pointing, Aiming, Homing};

    public abstract class AbstractTrial
    {
        public long TrialStartTime;
        // These two fields register the current block and Trial ID in the dataSingleton
        public int BlockID; 
        public int TrialID;

        public bool isSuccessful;
        // public int NumCollected;
        public TrialProgress TrialProgress;
        public AbstractTrial head; // This points to the start of the block of trials (if present)
        public bool isTail;
        public Data.Trial trialData;
        public Enclosure enclosure;
        public AbstractTrial next; // This points to the next Trial
        protected float _runningTime;
        public bool progressionComplete = false;


        protected AbstractTrial(int blockId, int trialId)
        {
            BlockID = blockId;
            TrialID = trialId;

            isSuccessful = false;

            if (blockId == -1 || trialId == -1) return;
            if (DataSingleton.GetData().Blocks.Count == 0) throw new Exception("No Trial in block");

            trialData = DataSingleton.GetData().Trials[trialId];

            if (trialData.Enclosure > 0)
            {
                enclosure = DataSingleton.GetData().Enclosures[trialData.Enclosure - 1];
            }

            // If the user hasn't set an Enclosure index we want to set the Enclosure to be unobtrusive
            // So the ground generates but nothing else.
            else
            {
                enclosure = new Enclosure
                {
                    WallHeight = 0,
                    WallColor = "1B5E20",
                    Sides = 4,
                    GroundTileSides = 0,
                    GroundTileSize = 0,
                    GroundColor = null,
                    Radius = 4,
                    Position = new List<float> { 0, 0 }
                };
            }
        }

        public virtual void PreEntry(TrialProgress t, bool first = true)
        {

            Debug.Log("Entering Trial: " + TrialID);
            if (head == this && first)
            {
                Debug.Log(string.Format("Entered Block: {0} at time: {1}", BlockID, DateTime.Now));
                t.ResetOngoing();
                t.successes = new List<int>();
                int NumBlocks = DS.GetData().Blocks.Count;
                t.NumCollectedPerBlock = new int[NumBlocks];
            }

            if (head == this && first == false)
            {
                Debug.Log(string.Format("Entered Block: {0} at time: {1}", BlockID, DateTime.Now));
                t.SpecialReset();
                t.successes = new List<int>();
                int NumBlocks = DS.GetData().Blocks.Count;
                t.NumCollectedPerBlock = new int[NumBlocks];
            }

            Debug.Log("Current Trial Increment: " + Config.DataSingleton.GetData().TrialInitialValue);

            if (t.TrialNumber < 2)
            {
                t.TimeSinceExperimentStart = 0.0f;
            }

            TrialStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            _runningTime = 0;
            TrialProgress = t;
            // TrialProgress.GetCurrTrial().NumCollected = 0;

        }

        public virtual void Update(float deltaTime)
        {
            TrialProgress.TimeSinceExperimentStart += deltaTime;
            _runningTime += deltaTime;
        }

        public void ResetTime()
        {
            if (TrialProgress.TrialNumber < 2)
            {
                TrialProgress.TimeSinceExperimentStart = 0.0f;
            }
            _runningTime = 0;
        }

        // Function for stuff to know that things have happened
        public virtual void Notify()
        {

        }

        // Load the next Trial
        public virtual void Progress()
        {
            Debug.Log("Progressing to next trial.");

            // Exiting current Trial
            TrialProgress.PreviousTrial = this;
            
            if (TrialProgress.instructional != 1)
            {
                var blockData = DS.GetData().Blocks[BlockID];
                if (blockData.TrialFunction != null)
                {
                    var trialFunction = blockData.TrialFunction;
                    var func =
                        typeof(Functions).GetMethod(trialFunction, BindingFlags.Static | BindingFlags.Public);
                    var result = func != null && (bool) func.Invoke(null, new object[] {TrialProgress});
                    
                    if (!result)
                    {
                        var tmp = next;
                        while (!tmp.isTail)
                        {
                            tmp = tmp.next;
                        }

                        Loader.Get().CurrTrial = tmp.next;
                        tmp.next.PreEntry(TrialProgress);
                        return;
                    }
                }

                // Data on how to choose the next Trial will be selected here.
                if (isTail)
                {
                    if (blockData.BlockFunction != null)
                    {
                        var tmp = blockData.BlockFunction;
                        var func = typeof(Functions).GetMethod(tmp, BindingFlags.Static | BindingFlags.Public);

                        var result = func != null && (bool) func.Invoke(null, new object[] {TrialProgress});

                        if (result)
                        {
                            Loader.Get().CurrTrial = head;
                            head.PreEntry(TrialProgress, false);
                            return;
                        }
                    }
                }
            }

            Loader.Get().CurrTrial = next;
            next.PreEntry(TrialProgress);
            progressionComplete = true;
        }

        protected void LoadNextSceneWithTimer(int environmentType)
        {
            Loader.Get().StartCoroutine(LoadNextAsyncScene(environmentType));
        }

        private IEnumerator LoadNextAsyncScene(int environmentType)
        {
            AsyncOperation ao = SceneManager.LoadSceneAsync(environmentType);
            TrialProgress.isLoaded = false;

            // Wait until the specified timeout to load the scene
            var timer = 0.0f;

            while (!ao.isDone && (!progressionComplete))
            {
                timer += Time.deltaTime;
                yield return null;
            }

            // Reset when loading is complete
            timer = 0.0f;
            progressionComplete = false;
        }
    }
}