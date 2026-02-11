using System;
using UnityEngine;

namespace VertiblockPass
{
    [Serializable]
    public struct Level
    {
        public int levelID;
        public GameObject levelPrefabs;
    }

    [CreateAssetMenu(menuName = "Vertiblock Pass/Level Data/New Level", fileName = "New Level")]
    public class VP_Level : ScriptableObject
    {
        [SerializeField] public Level level;
    }
}
