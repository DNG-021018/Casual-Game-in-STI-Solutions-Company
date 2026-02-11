using UnityEngine;

namespace VertiblockPass
{
    public class VP_PlayerState : MonoBehaviour
    {
        private float standingSqrThreshold = 0.01f;
        private VP_PlayerController _movement;

        public enum CubeState
        {
            Standing,
            Lying
        }

        public CubeState CurrentState { get; private set; }
        public bool IsStanding => CurrentState == CubeState.Standing;
        public bool IsLying => CurrentState == CubeState.Lying;

        void Start()
        {
            _movement = GetComponent<VP_PlayerController>();
            if (_movement == null)
            {
                return;
            }

            UpdateState();
        }

        public void UpdateState()
        {
            if (_movement == null) return;

            Transform child1 = _movement.Child1;
            Transform child2 = _movement.Child2;

            if (child1 == null || child2 == null)
            {
                return;
            }

            Vector3 p1 = child1.position;
            Vector3 p2 = child2.position;

            p1.y = 0f;
            p2.y = 0f;

            p1.x = Mathf.Round(p1.x * 2f) / 2f;
            p1.z = Mathf.Round(p1.z * 2f) / 2f;
            p2.x = Mathf.Round(p2.x * 2f) / 2f;
            p2.z = Mathf.Round(p2.z * 2f) / 2f;

            float sqDist = (p1 - p2).sqrMagnitude;

            if (sqDist <= standingSqrThreshold)
            {
                CurrentState = CubeState.Standing;
            }
            else
            {
                CurrentState = CubeState.Lying;
            }
        }
    }
}
