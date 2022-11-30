using UnityEngine;

namespace Controllers {
    public class SceneController : MonoBehaviour
    {
        void Awake()
        {
            foreach (Transform g in transform.GetComponentsInChildren<Transform>())
            {
                if (g.gameObject.CompareTag("Wall"))
                    g.gameObject.AddComponent<WallController>();
            }
        }

        void Start()
        {

        }

        void Update()
        {
            
        }
    }
}