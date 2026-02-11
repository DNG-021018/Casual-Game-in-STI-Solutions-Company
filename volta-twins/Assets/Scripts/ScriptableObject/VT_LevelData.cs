using UnityEngine;

namespace VoltaTwins
{
    [CreateAssetMenu(menuName = "Volta Twins/Level Data/New Level Data", fileName = "New Level Data")]
    public class VT_LevelData : ScriptableObject
    {
        [SerializeField] public VT_Level[] levels;
    }
}
