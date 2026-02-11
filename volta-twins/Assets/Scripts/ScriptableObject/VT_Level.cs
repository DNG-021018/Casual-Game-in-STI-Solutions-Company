using System;
using UnityEngine;

namespace VoltaTwins
{
    [Serializable]
    public struct Level
    {
        public int levelID;
        public GameObject levelPrefabs;
    }

    [CreateAssetMenu(menuName = "Volta Twins/Level Data/New Level", fileName = "New Level")]
    public class VT_Level : ScriptableObject
    {
        [SerializeField] public Level levels;
    }
}
