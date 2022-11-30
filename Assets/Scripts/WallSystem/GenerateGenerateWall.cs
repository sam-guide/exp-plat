using UnityEngine;
using UnityEngine.UI;
using DS = Config.DataSingleton;
using E = Main.Loader;

namespace WallSystem
{
    public class GenerateGenerateWall : MonoBehaviour
    {
        public GameObject Create;                       // This is the prefab of the GenerateWall object.
        public GameObject GenerateEnclosureFromFile;
        public Camera Cam;                              // This is the Main camera of the player.
        public Text Timer;                              // This exists as the timer text.
        public GameObject Player;

        public int currBlockId; // The id of the current block being tested.
        public int currTrialId; // The id of the current Trial being tested.

        // Current generate wall object that exists. This is intrinsically different from the Create object as that is a prefab while this is the instance.
        private GameObject _currCreate;

        // This is the current running timestamp that is outputted to the outputfile.
        private readonly float _timestamp;

        // Use this for initialization
        private void Start()
        {
            Create.transform.position = Vector3.zero;
            _currCreate = Instantiate(E.Get().CurrTrial.enclosure.Sides == 0 ? GenerateEnclosureFromFile : Create);
        }

        private void Update()
        {

            currBlockId = E.Get().CurrTrial.BlockID;
            currTrialId = E.Get().CurrTrial.TrialID;

            // HUD for the number of successful trials in the current Block
            if (DS.GetData().Blocks[currBlockId].ShowNumSuccesses | DS.GetData().Trials[currTrialId].ShowNumSuccesses)
            {
                var trialsuccessText = GameObject.Find("TrailSuccesses").GetComponent<Text>();
                trialsuccessText.text = "Successful Trials: " + E.Get().CurrTrial.TrialProgress.NumSuccess;
            }
            
            // HUD for the number of goals found in the current Trial 
            /*
            if (DS.GetData().Blocks[currBlockId].ShowTrialTotal | DS.GetData().Trials[currTrialId].ShowTrialTotal)
            {
                var trialtotalText = GameObject.Find("TrialTotal").GetComponent<Text>();
                trialtotalText.text = "Goals Found In Trial: " + E.Get().CurrTrial.NumCollected;
            }
            */

            // HUD for the number of goals found in the current Block
            /*
            if (DS.GetData().Trials[currTrialId].ShowBlockTotal | DS.GetData().Blocks[currBlockId].ShowBlockTotal)
            {
                var blocktotalText = GameObject.Find("BlockTotal").GetComponent<Text>();
                blocktotalText.text = "Goals Found In Block: " + E.Get().CurrTrial.TrialProgress.NumCollectedPerBlock[currBlockId];
            }
            */
        }
    }
}
