using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public static class Spatial
    {
        public static Vector2 RandomPositionInRadius(int radius)
        {
            return(Random.insideUnitCircle * radius * 0.9f);
        }

        public static float GetDistanceToPlayer(Camera player, GameObject source)
        {
            return(
                UnityEngine.Vector3.Distance(
                    source.GetComponent<Collider>().ClosestPoint(player.transform.position), 
                    player.transform.position
                )
            );
        }

        public static float AngleTowards(Transform playerTransform, Vector3 targetPosition)
        {
            targetPosition = playerTransform.InverseTransformDirection(targetPosition);
            float angle = Mathf.Atan2(targetPosition.x, targetPosition.z) * Mathf.Rad2Deg;
            return(angle + 180f);
        }

        // Get the angle in 2D (-180, 180) --> sets the elevation of the object at that of the player
        public static float GetAngleToPlayer2D(Camera player, GameObject source)
        {
            UnityEngine.Vector3 obj2player = new UnityEngine.Vector3(source.transform.position.x, player.transform.position.y, source.transform.position.z) - player.transform.position;
            // float angle = UnityEngine.Vector3.Angle(obj2player, player.transform.forward); // Other solution (angle between 0 and 180)
            // float angle = (180 / Mathf.PI) * (Mathf.Acos(UnityEngine.Vector3.Dot(player.transform.forward, source.transform.forward))); // Other solution (angle between 0 and 180)

            obj2player = player.transform.InverseTransformDirection(obj2player);
            float angle = Mathf.Atan2(obj2player.x, obj2player.z) * Mathf.Rad2Deg;
            return(angle);
        }

        public static Quaternion QuaternionAverage(this List<Quaternion> quaternions)
        {
            if(quaternions == null || quaternions.Count < 1)
                return Quaternion.identity;
    
            if(quaternions.Count < 2)
                return quaternions[0];
    
            float weight = 1.0f / (float)quaternions.Count;
            Quaternion avg = Quaternion.identity;
    
            for(int i = 0; i < quaternions.Count; i++)
                avg *= Quaternion.Slerp(Quaternion.identity, quaternions[i], weight);
    
            return avg;
        }
    }
}