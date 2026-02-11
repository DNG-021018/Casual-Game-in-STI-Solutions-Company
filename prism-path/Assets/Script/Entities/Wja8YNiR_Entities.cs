using UnityEngine;

namespace Wja8YNiR_PrismPath
{
    public enum EntityType { Mirror, Obstacle, Button, Plant }

    [System.Serializable]
    public abstract class Wja8YNiR_Entities : MonoBehaviour
    {
        [SerializeField]
        public EntityType Type;
    }
}
