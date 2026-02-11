using System.Collections;
using UnityEngine;

namespace NightEscape
{
    public class NE_Key : MonoBehaviour
    {
        [Header("Door")]
        [SerializeField] private NE_Door door;

        [Header("Key Mesh")]
        [SerializeField] private GameObject keyMesh;

        [Header("VFX")]
        [SerializeField] private ParticleSystem pickUpVFX;
        [SerializeField] private ParticleSystem buffVFX;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip lootCLip;

        private bool isCollected = false;

        private NE_AudioManager audioManager => NE_AudioManager.Instance;

        void Awake()
        {
            pickUpVFX.gameObject.SetActive(false);
        }

        void OnTriggerEnter(Collider other)
        {
            if (isCollected) return;
            isCollected = true;
            if (!other.CompareTag(NE_SafetyKey.KEY_TAG_PLAYER)) return;

            if (door != null)
            {
                door.Open();
            }

            if (audioManager != null && lootCLip != null)
            {
                audioManager.SetBgmVolume();
                audioManager.PlaySfx(lootCLip, 6f);
            }

            pickUpVFX.gameObject.SetActive(true);
            pickUpVFX.Play();
            keyMesh.gameObject.SetActive(false);
            buffVFX.gameObject.SetActive(false);
            StartCoroutine(DisableKey());
        }

        IEnumerator DisableKey()
        {
            yield return new WaitForSeconds(pickUpVFX.main.duration + 0.2f);
            gameObject.SetActive(false);
        }
    }
}
