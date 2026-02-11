using UnityEngine;

namespace VertiblockPass
{
    [CreateAssetMenu(menuName = "Vertiblock Pass/Level Data/New Level Data", fileName = "New Level Data")]
    public class VP_LevelData : ScriptableObject
    {
        [SerializeField] public VP_Level[] levels;
    }
}
