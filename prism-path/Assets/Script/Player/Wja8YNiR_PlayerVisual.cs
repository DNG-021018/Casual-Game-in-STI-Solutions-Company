using System;
using UnityEngine;

namespace Wja8YNiR_PrismPath
{
    public class Wja8YNiR_PlayerVisual : MonoBehaviour
    {
        [SerializeField] private Animator Wja8YNiR_animator;
        private int IsShootingHash = Animator.StringToHash("IsShooting");
        [SerializeField] Wja8YNiR_LaserBeam beam;

        // Hàm chạy trong event animation
        private void Start()
        {
            Wja8YNiR_animator = GetComponent<Animator>();
            Wja8YNiR_GamePlay.Shoot += StartShooting;
            beam.OnLaserBlocked += Wja8YNiR_StopBeam;
        }

        private void OnDisable()
        {
            Wja8YNiR_GamePlay.Shoot -= StartShooting;
            beam.OnLaserBlocked -= Wja8YNiR_StopBeam;
        }

        private void StartShooting()
        {
            Wja8YNiR_animator.SetBool(IsShootingHash, true);
        }

        public void Wja8YNiR_StartBeam()
        {
            beam.StartShooting();
        }

        public void Wja8YNiR_StopBeam()
        {
            Wja8YNiR_animator.SetBool(IsShootingHash, false);
        }

    }
}
