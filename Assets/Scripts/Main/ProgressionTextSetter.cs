using Config;
using UnityEngine;
using UnityEngine.UI;
using E = Main.Loader;

namespace Main
{
    public class ProgressionTextSetter : MonoBehaviour
    {
        //Sets the image of the loading screen.
        public void Start()
        {
            Debug.Log("Entering Loading Screen: " + E.Get().CurrTrial.TrialID);
            var filePath = DataSingleton.GetData().SpritesPath + E.Get().CurrTrial.trialData.FileLocation;
            GetComponent<RawImage>().texture = Img2Sprite.LoadTexture(filePath);
        }
    }
}