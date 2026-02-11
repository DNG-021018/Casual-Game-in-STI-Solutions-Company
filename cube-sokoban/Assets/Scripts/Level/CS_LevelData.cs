using UnityEngine;

namespace CubeSokoban
{
    [CreateAssetMenu(menuName = "Cube Sokoban/Level Data/New Level Data", fileName = "New Level Data")]
    public class CS_LevelData : ScriptableObject
    {
        [SerializeField] public CS_Level[] levels;
    }
}
