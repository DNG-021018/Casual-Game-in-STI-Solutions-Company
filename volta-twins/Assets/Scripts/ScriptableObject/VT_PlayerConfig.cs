using UnityEngine;

namespace VoltaTwins
{
    [CreateAssetMenu(fileName = "New Player", menuName = "Volta Twins/Player/New Player")]
    public class VT_PlayerConfig : ScriptableObject
    {
        public float moveSpeed;
        public PlayerType type;
    }
}
