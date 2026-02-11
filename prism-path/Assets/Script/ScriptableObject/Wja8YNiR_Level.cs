using System;
using UnityEngine;

namespace Wja8YNiR_PrismPath
{
    [Serializable]
    public struct Level
    {
        public int levelID;
        public GameObject levelPrefabs;
        public float timeLimit;
        public int mirrorLimit;
        public int FirstStarRequire;
        public int SecondStarRequire;
        public int ThirdStarRequire;
    }

    [CreateAssetMenu(menuName = "Level Data/New Level", fileName = "Level ")]
    public class Wja8YNiR_Level : ScriptableObject
    {
        [SerializeField] public Level levels;
    }
}
