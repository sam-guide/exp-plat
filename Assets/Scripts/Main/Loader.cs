using System;
using System.IO;
using Config;
using Trial;
using UnityEngine;
using UnityEngine.UI;
using C = Config.Constants;
using DS = Config.DataSingleton;
using Substitution;

namespace Main
{
    // Main entry point of the app as well as the game object that stays alive for all scenes.
    public class Loader : MonoBehaviour
    {
        // Singleton function
        public static Loader Get()
        {
            return GameObject.Find("Loader").GetComponent<Loader>();
        }
        public InputField[] Fields; // These are an array of the fields given from the field trials

        private static float _timer = 0;

        public AbstractTrial CurrTrial;

        private void Start()
        {
            DontDestroyOnLoad(this);
            CurrTrial = new FieldTrial(Fields); // Initialize the default field Trial
        }

        // This function initializes the Data.singleton files
        public static bool ExternalActivation(string inputFile)
        {
            if (!inputFile.Contains(".json")) return false;
            DS.Load(inputFile);
            Directory.CreateDirectory(C.OutputDirectory);
            return true;
        }

        private void Update()
        {
            CurrTrial.Update(Time.deltaTime);
        }

        public static void LogHeaders()
        {
            using (StreamWriter writer = new StreamWriter("Assets/OutputFiles~/" + DS.GetData().OutputFile, false))
            {
                writer.Write(
                    "Nom, Prenom, Age, Sex, TimeStamp, " +
                    "BlockIndex, TrialIndex, TrialInBlock, Instructional, Scene, Enclosure, " + 
                    "Modality, DistanceLink, AngleLink, " + 
                    "PositionX, PositionY, PositionZ, TargetX, TargetY, TargetZ, DistanceToTarget, " +
                    "RotationY, AngleToTargetH, " + 
                    "ValidationType, FoundTarget" +
                    "\n"
                );
                writer.Flush();
                writer.Close();
            }
        }

        public static void LogData(TrialProgress tp, long trialStartTime, Transform t, int targetFound = 0)
        {
            // Don't output anything if the Y position is at default (avoids incorrect output data)
            if (t.position.y != -1000 && (targetFound == 1 || _timer > 1f / (DS.GetData().OutputTimesPerSecond == 0 ? 1000 : DS.GetData().OutputTimesPerSecond)))
            {
                using (StreamWriter writer = new StreamWriter("Assets/OutputFiles~/" + DS.GetData().OutputFile, true))
                {
                    string PositionX = tp.instructional == 1 ? "NA" : t.position.x.ToString();
                    string PositionZ = tp.instructional == 1 ? "NA" : t.position.z.ToString();
                    string PositionY = tp.instructional == 1 ? "NA" : t.position.y.ToString();
                    string RotationY = tp.instructional == 1 ? "NA" : t.eulerAngles.y.ToString();
                    
                    var timeSinceExperimentStart = DateTimeOffset.Now.ToUnixTimeMilliseconds() - DataSingleton.GetData().ExperimentStartTime;
                    var timeSinceTrialStart = DateTimeOffset.Now.ToUnixTimeMilliseconds() - trialStartTime;

                    SubstitutionSource substitutionSource = tp.targetBeacon.GetSubstitutionSource();

                    // string audioType = tp.targetBeacon.audioSubstitution.audioFeedbackType.ToString();
                    string distanceEncoding = substitutionSource.GetDistanceEncoding().ToString();
                    string angleEncoding = substitutionSource.GetAngleEncoding().ToString();

                    float distanceToTarget = substitutionSource.distanceToPlayer;
                    float angleToTargetH = substitutionSource.angleToPlayer;

                    var date = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt");

                    string str = $"{tp.nom}, {tp.prenom}, {tp.age}, {tp.sex}, {date}, " +
                        $"{tp.BlockID + 1}, {tp.TrialID + 1}, {tp.TrialNumber + 1}, {tp.instructional}, {tp.EnvironmentType}, {tp.CurrentEnclosureIndex + 1}, " +
                        $"{tp.targetBeacon.modality}, {distanceEncoding}, {angleEncoding}, " + 
                        $"{PositionX}, {PositionY}, {PositionZ}, {tp.targetX}, {tp.targetY}, {tp.targetZ}, {distanceToTarget}, " +
                        $"{RotationY}, {angleToTargetH}, " +
                        $"{tp.validationType.ToString()}, {targetFound}";               
                    writer.Write(str + "\n");
                    writer.Flush();
                    writer.Close();
                }
                _timer = 0;
            }
            _timer += Time.deltaTime;
        }
    }
}
