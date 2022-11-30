using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controllers
{
    public class OrbitController : MonoBehaviour
    {
        /** TODO: https://answers.unity.com/questions/1614738/how-do-make-an-object-move-in-a-circle-around-the.html **/

        // Drag & drop the orbit anchor in the inspector
        [SerializeField] private Transform anchor;
        public float rotationSpeed = 0.7f;
        public float radius = 10f;
        public float yOffset = 0f;
        private float angle;

        void Awake()
        {
            anchor = transform.parent;
        }

        private void LateUpdate()
        {
            transform.position = anchor.position + new Vector3(Mathf.Cos(angle) * radius, yOffset, Mathf.Sin(angle) * radius);
            angle += Time.deltaTime * rotationSpeed;
        }
    }
}