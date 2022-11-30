using UnityEngine;

namespace Controllers
{
    public class WaypointFollowController : MonoBehaviour
    {
        [SerializeField] private WaypointController waypoints;
        private Transform currentWaypoint;
        [SerializeField] private float speed = 4.5f;
        [SerializeField] private float distanceThreshold = 0.1f;

        void Start()
        {
            /** Initializing by jumping the object at the first waypoint's location **/
            currentWaypoint = waypoints.GetNextWaypoint(currentWaypoint);
            JumpToWaypoint(currentWaypoint);

            /** Getting the actual next waypoint on the path **/
            SelectNextWaypoint();
        }

        // Update is called once per frame
        void Update()
        {
            /* Moving to the next waypoint */
            MoveToNextWaypoint();

            /* When the target waypoint is reached, select the next one */
            if (Vector3.Distance(transform.position, currentWaypoint.position) < distanceThreshold)
                SelectNextWaypoint();
        }

        private void MoveToNextWaypoint()
        {
            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint.position, speed * Time.deltaTime);
        }

        private void SelectNextWaypoint()
        {
            currentWaypoint = waypoints.GetNextWaypoint(currentWaypoint);
            transform.LookAt(currentWaypoint);
            // transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(currentWaypoint.position), rotationSpeed * Time.deltaTime);
        }

        private void JumpToWaypoint(Transform waypoint)
        {
            transform.position = waypoint.position;
        }
    }
}