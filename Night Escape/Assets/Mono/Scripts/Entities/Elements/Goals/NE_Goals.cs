using System;
using UnityEngine;

namespace NightEscape
{
    public class NE_Goals : MonoBehaviour
    {
        [SerializeField] private ParticleSystem[] goalVFX;
        [SerializeField] private AudioClip goalSound;

        void Start()
        {
            if (goalVFX == null || goalVFX.Length == 0)
            {
                goalVFX = GetComponentsInChildren<ParticleSystem>();
            }

            foreach (ParticleSystem ps in goalVFX)
            {
                ps.gameObject.SetActive(false);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(NE_SafetyKey.KEY_TAG_PLAYER)) return;

            NE_PlayerController player = other.GetComponent<NE_PlayerController>();
            NE_GameManager.Instance.SetState(GameState.Win);
            if (player != null)
            {
                player.ReachGoal();
            }

            foreach (ParticleSystem ps in goalVFX)
            {
                ps.gameObject.SetActive(true);
                ps.Play();
            }
        }
    }
}
