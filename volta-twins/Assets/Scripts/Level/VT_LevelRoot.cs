using UnityEngine;

namespace VoltaTwins
{
    public class VT_LevelRoot : MonoBehaviour
    {
        [Header("Players in this level")]
        public VT_PlayerController bluePlayer;
        public VT_PlayerController redPlayer;

        [Header("Goals in this level (optional)")]
        public VT_GoalsButton[] goals;
    }
}
