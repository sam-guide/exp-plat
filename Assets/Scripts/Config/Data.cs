using System;
using System.Collections.Generic;
using UnityEngine;
using Controllers.Beacons;
using Substitution;

namespace Config
{
    [Serializable]
    public class Data
    {
        public long ExperimentStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        public string SpritesPath = Application.streamingAssetsPath + "/2D_Objects/";
        public string AutoRunConfigPath = Application.streamingAssetsPath + "/AutoRun_Config/";
        public bool TimingVerification; // whether or not to turn on timing diagnostics
        public int TrialInitialValue = 1; // value from which trials start incrementing in the config file
        public float TrialLoadingDelay = 3.0f; // The delay (in seconds) before a Trial loads. Used to ensure a consitent loading time between trials.
        public float IgnoreUserInputDelay = 1.0f; // Ignore user input for _ seconds at the beginning of the trial. Used to avoid accidentally ending the Trial
        public int OutputTimesPerSecond; // Frequency (per second) of data logging
        public Character CharacterData; // Contains all data available for the Main player (see model below)
        public string OutputFile; // The output file of the character's movements during an experiment (created automatically)
        public float WorldScale; // Scale (increase) the size of the environment to match its scale to that of the real-world area of the experiment
        public string MotionController; // What device is used as motion intput controller: KM (Keyboard-Mouse), G4 (Polhemus G4), VICON, ...
        public List<string> MotorOrder; // Ordered list of the motors used for a given subject

        public List<Beacon> Beacons;
        public List<LandMark> Landmarks;
        public List<Trial> Trials;
        public List<Enclosure> Enclosures;
        public List<BlockData> Blocks;
        public List<int> BlockOrder;

        [Serializable]
        public class BlockData
        {
            public string BlockGoal;                    // percentage ___SPACE___ number. This is very arbitrary.
            public string BlockFunction;                // The function name (if not present, we assume its always true)
            public string TrialGoal;                    // percentage ___SPACE___ number. This is very arbitrary.
            public string TrialFunction;                // The function name (if not present, we assume its always false)

            public string BlockName;                    // Name (outputed at the end of the Block)
            public string BlockInfoText;                // Text displayed on the screen
            public int IsPressToProgress;               // Needs to press the validation key to move to the next trial after completing one
            public string ValidationType;               // Set to 1 if the Trial has to be validated manually
            public string BlockType;                    // OneD, TwoD (default), ThreeD (not implemented)
            public string TaskType;                     // Type of task: Navigation (default), Homing
            public List<float> SpawnDistanceToPlayer;   // Range (or list) of distances the target can spawn (relative to the player's position)
            public List<float> SpawnAngleToPlayer;      // Range (or list) of anges the target can spawn at (relative to the player's position)
            public int IsMasked;                        // Trials of this block include masking (signals disappears after a condition)
            public string Notes;                        // Notes about the given block
            public int Replacement;                     // Integer value representing replacement
            public List<RandomData> RandomlySelect;     // Array that contains all the possible random values
            public List<int> TrialOrder;                // Trial order (-1 means random)

            public bool ShowNumSuccesses;               // Whether or not to display the number of successful trials
            public bool ShowBlockTotal;                 // Whether or not to display the amount of goals/pickups collected (resets each block)
            public bool ShowTrialTotal;                 // Whether or not to display the amount of goals/pickups collected (resets each Trial)

        }

        [Serializable]
        public class RandomData
        {
            public List<int> Order;
        }

        [Serializable]
        public class Trial
        {
            // public int Rotate;                       // How long the delay can last in the rotation
            public int Instructional;                   // Set to 1 if Trial is instructional
            public string FileLocation;                 // Is not null if FileLocation exists (Image for 1D trials)
            public int Scene;                           // This is the environment type referenced.
            public int TrialTime;                       // Allotted amount of time
            public string TrialEndKey;                  // The key press which will end the current Trial.
            public string TrialInfoText;                // Text displayed on the screen
            public string DisplayImage;                 // Is not null if FileLocation exists (Image for 1D trials)
            public EnclosureData Map;                   // The Map saved EnclosureData
            public List<int> BeaconList;
            public int Enclosure;                       // The index of the enclosure to load (starts from 1, 0 is reserved for no enclosure)
            public List<int> LandMarks;                 // List of landmarks
            public int Quota;                           // The quota that the person needs to pick up before the next Trial is switched too
            public List<float> StartPosition;           // The start position of the character (usually 0, 0) If left empty (ie. "[]") start position is random
            public float StartFacing;                   // The starting angle of the character (in degrees). if set to -1 start facing will be random
            public bool ExitButton;                     // When this is set to true a button that says "Exit Experiment" will appear at the bottom of screen, when pressed application will close
            public bool ShowBlockTotal;                 // Whether or not to display the amount of goals/pickups collected (resets each block)
            public bool ShowTrialTotal;                 // Whether or not to display the amount of goals/pickups collected (resets each Trial)
            public bool ShowNumSuccesses;               // Whether or not to display the number of successful trials
        }

        [Serializable]
        public class Enclosure
        {
            public string EnclosureName; // Non functional (user can use this label to keep track of their enclosures)
            public int Sides; // Number of sides present in the Trial.
            public int Radius; // Radius of the walls
            public float WallHeight; // This is the wall height
            public string WallColor; // HEX color of the walls
            public int GroundTileSides; // Number of sides on the ground pattern shapes - 0 for no tiles, 1 for solid color.
            public double GroundTileSize; // Relative size of the floor tiles - Range from 0 to 1
            public string GroundColor; // Colour of the ground
            public List<float> Position; // 2d position vector
        }

        [Serializable]
        public class Beacon
        {
            public string Tag; // The name of the pickup item
            public string Object; // The name of the prefab
            public List<float> Position;
            public List<float> Rotation;
            public List<float> Scale;
            public string TriggerSound; // The file path of the sound played onTrigger
            public string BeaconType; // The beacon type: Target, Waypoint, Landmark, Center
            public string FeedbackModality;
            public string AudioFeedbackType; // Clip, Substitution
            public string AudioClipName;
            public string PatchName;

            public string DistanceEncoding;
            public float? DistanceMin, DistanceMax;
            public string DistanceToIntensityLink;
            public float? IntensityMin, IntensityMax;
            public int? IntensitySteps;
            public string DistanceToIBILink;
            public float? IBIMin, IBIMax;
            public int? IBISteps;
            public string AngleEncoding;
            public float? AngleMin, AngleMax;
            public string AngleToFrequencyLink;
            public float? FrequencyMin, FrequencyMax;
            public int? FrequencySteps;

            public Vector3 PositionVector { get => Position.Count == 0 ? Vector3.zero : new Vector3(Position[0], Position[1], Position[2]); }
            public Vector3 RotationVector { get => Rotation.Count == 0 ? Vector3.zero : new Vector3(Rotation[0], Rotation[1], Rotation[2]); }
            public Vector3 ScaleVector { get => Scale.Count == 0 ? Vector3.zero : new Vector3(Scale[0], Scale[1], Scale[2]); }
        }

        [Serializable]
        public class Character
        {
            public float MovementSpeed; // The movement speed of the character
            public float RotationSpeed; // The rotation speed of the character
            // public float GoalRotationSpeed;
            public float Height; // The height of the character
            public float DistancePickup; // The min distance of the pickup to the character
        }

        [Serializable]
        public class LandMark
        {
            public string Object;
            public List<float> Position;
            public List<float> Rotation;
            public List<float> Scale;

            public Vector3 PositionVector { get => Position.Count == 0 ? Vector3.zero : new Vector3(Position[0], Position[1], Position[2]); }
            public Vector3 RotationVector { get => Rotation.Count == 0 ? Vector3.zero : new Vector3(Rotation[0], Rotation[1], Rotation[2]); }
            public Vector3 ScaleVector { get => Scale.Count == 0 ? Vector3.zero : new Vector3(Scale[0], Scale[1], Scale[2]); }
        }

        [Serializable]
        public class EnclosureData
        {
            public List<float> TopLeft; //The top left of the enclosure
            public float TileWidth;
            public List<string> Map;
            public string Color;
        }

        // [MAR]: is this really needed ?
        public class Point
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Z { get; set; }
        }
    }
}