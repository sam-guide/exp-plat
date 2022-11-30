using Main;
using SFB;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Debug = UnityEngine.Debug;
using DS = Config.DataSingleton;

namespace Trial
{
    // This is the only Trial in which the current data is null. This is so that we have uniform setup
    public class FieldTrial : AbstractTrial
    {
        private readonly InputField[] _fields;

        // Here we construct the entire linked list structure.
        public FieldTrial(InputField[] fields) : base(-1, -1)
        {
            _fields = fields;

            // This block of code will use the default configuration file if
            // using webGL or Android (can add others in if statement) 
            // Otherwise calls the directory picker to select the configuration file.
    
            var AutoRunConfigDir = Application.streamingAssetsPath + "/AutoRun_Config/";
            var files = new string[0];
            if (Directory.Exists(AutoRunConfigDir))
            {
                files = Directory.GetFiles(AutoRunConfigDir);
            }

            if (files.Length > 0)
            {
                var defaultConfig = AutoRunConfigDir + Path.GetFileName(files[0]);
                Loader.ExternalActivation(defaultConfig);
            }

            else
            {
                while (true)
                {
                    // Here we're using: https://github.com/gkngkc/UnityStandaloneFileBrowser because it was easier than rolling our own
                    string[] paths = StandaloneFileBrowser.OpenFilePanel("Choose configuration file", "Configuration_Files", "", false);
                    string path = paths[0];
                    if (Loader.ExternalActivation(path)) break;
                }
            }

            TrialProgress = new TrialProgress();
            _fields = fields;

        }


        //We are gonna generate all the trials here.
        private void GenerateTrials()
        {
            AbstractTrial currentTrial = this;
            foreach (var i in DS.GetData().BlockOrder)
            {
                var l = i - 1;
                var block = DS.GetData().Blocks[l];
                var newBlock = true;
                AbstractTrial currHead = null;

                var tCnt = 0;
                foreach (var j in block.TrialOrder)
                {
                    var k = j - 1;
                    AbstractTrial t;

                    // Here we have a picture as a Trial.
                    if (k < 0)
                    {
                        t = new RandomTrial(l, k);
                    }
                    else
                    {
                        var trialData = DS.GetData().Trials[k];

                        if (trialData.Instructional == 1) t = new InstructionalTrial(l, k); // trialData.FileLocation != null
                        else t = new ThreeDTrial(l, k);
                    }
                    if (newBlock) currHead = t;

                    t.isTail = tCnt == block.TrialOrder.Count - 1;
                    t.head = currHead;

                    currentTrial.next = t;

                    currentTrial = currentTrial.next;

                    newBlock = false;
                    tCnt++;
                }

                currentTrial.next = new CloseTrial(-1, -1);
            }
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (StartButton.clicked == true)
            {
                // Sets the output file name as the desired one.
                var Field1Text = _fields[0].transform.GetComponentsInChildren<Text>()[1];
                TrialProgress.nom = Field1Text.text;

                var Field2Text = _fields[1].transform.GetComponentsInChildren<Text>()[1];
                TrialProgress.prenom = Field2Text.text;

                var Field3Text = _fields[2].transform.GetComponentsInChildren<Text>()[1];
                TrialProgress.age = Field3Text.text;

                var Field4Text = _fields[3].transform.GetComponentsInChildren<Text>()[1];
                TrialProgress.sex = Field4Text.text;

                DS.GetData().OutputFile = TrialProgress.nom + "_" +
                                          TrialProgress.prenom + "_" +
                                          TrialProgress.age + "_" +
                                          TrialProgress.sex + "_" + 
                                          DateTime.Now.ToString("yyyy-MM-dd-HH.mm.ss") + ".csv";

                GenerateTrials();

                Loader.LogHeaders();

                Progress();
            }

        }

        public override void Progress()
        {
            Loader.Get().CurrTrial = next;
            next.PreEntry(TrialProgress);
        }
    }
}