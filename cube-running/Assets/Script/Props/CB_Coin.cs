using UnityEngine;

namespace CB_CubeRunner
{
    public class CB_Coin : MonoBehaviour
    {
        [SerializeField] AudioClip collectSfx;
        [SerializeField] float rotateSpeed = 90f;
        CB_AudioManager audioManager;

        private void Start()
        {
            audioManager = CB_AudioManager.Instance;
        }

        void Update()
        {
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.Self);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<CR_PlayerController>() != null)
            {
                CB_GameManager.Instance?.AddCoin(1);

                if (audioManager != null && collectSfx != null)
                    audioManager.PlaySfx(collectSfx, 0.5f);

                Destroy(gameObject);
            }
        }
    }
}
