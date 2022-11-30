using UnityEngine;

namespace Controllers
{
    public class WaypointController : MonoBehaviour
    {
        [Range(0f, 5f)]
        [SerializeField] private float waypointSize = 2f;

        void OnDrawGizmos()
        {
            foreach (Transform t in transform)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(t.position, waypointSize);
            }

            Gizmos.color = Color.blue;
            for (int i = 0; i < transform.childCount - 1; i++)
            {
                Gizmos.DrawLine(transform.GetChild(i).position, transform.GetChild(i+1).position);
            }
            Gizmos.DrawLine(transform.GetChild(transform.childCount - 1).position, transform.GetChild(0).position);
        }

        public Transform GetNextWaypoint(Transform currentWaypoint)
        {
            if (currentWaypoint == null)
               return transform.GetChild(0); // TODO: return closest Waypoint or closest point on paths between Waypoints

            if (currentWaypoint.GetSiblingIndex() < transform.childCount - 1) // We're inside the loop
                return transform.GetChild(currentWaypoint.GetSiblingIndex() + 1);
            else // We're at the last waypoint
                return transform.GetChild(0);
        }
    }
}