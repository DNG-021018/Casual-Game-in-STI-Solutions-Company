using System.Collections;
using UnityEngine;

namespace VertiblockPass
{
    public class VP_TileWeak : VP_TilesBase
    {
        private BoxCollider groundCollider;
        private Rigidbody _rb;
        private bool _broken;

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();

            groundCollider = GetComponent<BoxCollider>();

            if (_rb != null)
            {
                _rb.isKinematic = true;
                _rb.useGravity = false;
            }
        }

        public override void HandleCubeEnter(VP_PlayerController player, VP_PlayerState state)
        {
            if (_broken) return;
            if (!state.IsStanding) return;

            _broken = true;

            _rb.isKinematic = false;
            _rb.useGravity = true;

            _rb.AddForce(Vector3.down * 5, ForceMode.Impulse);

            groundCollider.isTrigger = true;

            player.FallStraightDown();
        }
    }
}
