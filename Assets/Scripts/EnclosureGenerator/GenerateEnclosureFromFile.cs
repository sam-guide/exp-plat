using Config;
using UnityEngine;
using WallSystem;
using Controllers;
using Utils;
using DS = Config.DataSingleton;
using L = Main.Loader;

public class GenerateEnclosureFromFile : MonoBehaviour
{
    // Use this for initialization
    private void Start()
    {
        var m = L.Get().CurrTrial.trialData.Map;
        var z = m.TopLeft[1];

        // Goes through each map and initializes it based on stuff.
        foreach (var row in m.Map)
        {
            var x = m.TopLeft[0];

            foreach (var col in row.ToCharArray())
            {
                if (col == 'w')
                {
                    var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    obj.GetComponent<Renderer>().sharedMaterial.color = Misc.GetColour(m.Color);
                    obj.transform.localScale = new Vector3(m.TileWidth, L.Get().CurrTrial.enclosure.WallHeight, m.TileWidth);
                    obj.transform.position = new Vector3(x, L.Get().CurrTrial.enclosure.WallHeight * 0.5f, z);
                }
                else if (col == 's')
                {
                    // Debug.Log(x + " " + z);
                    GameObject.Find("Participant").GetComponent<PlayerController>().ExternalStart(x, L.Get().CurrTrial.enclosure.WallHeight * 0.5f, z, true);
                }
                else if (col != '0')
                {
                    // TODO: This code should be shared with PickupGenerator.cs - this class might also just be deadcode.
                    var val = col - '0';
                    var beacon = DS.GetData().Beacons[val - 1];

                    GameObject prefab;
                    GameObject obj;

                    prefab = (GameObject)Resources.Load("3D_Objects/" + beacon.Object, typeof(GameObject));
                    obj = Instantiate(prefab);
                    // obj.AddComponent<RotateBlock>();
                    // obj.GetComponent<Renderer>().material.color = Misc.GetColour(beacon.Color);

                    obj.transform.localScale = beacon.ScaleVector;
                    obj.transform.position = new Vector3(x, 0.5f, z);
                }

                x += m.TileWidth;
            }

            z -= m.TileWidth;
        }
    }
}
