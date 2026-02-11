using UnityEngine;
using UnityEngine.UI;

namespace Bowmancer
{
    public abstract class B_BaseCheckPoint : MonoBehaviour
    {
        [SerializeField] protected float duration = 1f;
        [SerializeField] protected Image durationVisual;

        protected Collider triggerCollider;
        protected float currentTime = 0f;
        protected bool playerInZone = false;
        protected bool hasTriggered = false;
        protected B_PlayerController playerInside = null;
        protected B_GameManager _gameManager;

        void Awake()
        {
            _gameManager = B_GameManager.Instance;
        }

        void Start()
        {
            triggerCollider = GetComponent<CapsuleCollider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(B_SafetyKey.TAG_PLAYER))
            {
                playerInZone = true;
                playerInside = other.GetComponent<B_PlayerController>();

                if (currentTime == 0f)
                {
                    currentTime = 0f;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(B_SafetyKey.TAG_PLAYER))
            {
                playerInZone = false;
                playerInside = null;

                if (durationVisual != null)
                    durationVisual.fillAmount = Mathf.Clamp01(currentTime / duration);
            }
        }

        private void Update()
        {
            if (hasTriggered || !playerInZone || playerInside == null)
                return;

            currentTime += Time.deltaTime;

            if (durationVisual != null)
            {
                durationVisual.fillAmount = Mathf.Clamp01(currentTime / duration);
            }

            if (currentTime >= duration)
            {
                OnUpgradeActivated();
            }
        }

        protected virtual void OnUpgradeActivated()
        {
            hasTriggered = true;
            triggerCollider.enabled = false;
            _gameManager.SetState(GameState.PickupUpgrade);
        }
    }
}
