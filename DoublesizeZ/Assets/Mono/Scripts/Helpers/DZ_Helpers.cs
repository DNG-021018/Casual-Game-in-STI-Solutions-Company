using UnityEngine;

namespace DoublesideZ
{
    public static class DZ_Helpers
    {
        public static Transform FindInChildren(Transform parent, string targetName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == targetName)
                    return child;

                Transform result = FindInChildren(child, targetName);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}
