using System;
using UnityEngine;

namespace CubeSokoban
{
    [Serializable]
    public struct Level
    {
        public int levelID;
        public GameObject levelPrefabs;
        public int requiredGoals;
    }

    [CreateAssetMenu(menuName = "Cube Sokoban/Level Data/New Level", fileName = "New Level")]
    public class CS_Level : ScriptableObject
    {
        [SerializeField] public Level level;
    }
}
