using UnityEngine;

namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_Utils : MonoBehaviour
    {
        public static Vector3 ScreenToWorld(Camera camera, Vector3 position)
        {
            position.z = camera.nearClipPlane;
            return camera.ScreenToWorldPoint(position);
        }
    }
}
