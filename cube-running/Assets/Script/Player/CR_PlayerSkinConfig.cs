using System;
using UnityEngine;

namespace CB_CubeRunner
{
    [Serializable]
    public struct SkinStruct
    {
        public int ID;
        public CR_PlayerVisual visual;

        public Sprite icon;
        public string displayName;

        public bool isDefaultSkin;
        public bool isUnlock;
    }

    [CreateAssetMenu(fileName = "New Skin Config", menuName = "Cube Running/Player/Skin")]
    public class CR_PlayerSkinConfig : ScriptableObject
    {
        [SerializeField] public SkinStruct[] skinConfig;
    }
}
