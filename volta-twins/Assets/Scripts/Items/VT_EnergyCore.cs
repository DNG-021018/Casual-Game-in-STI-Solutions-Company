using UnityEngine;

namespace VoltaTwins
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class VT_EnergyCore : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private int maxBounce = 4;

        [Header("VFX")]
        [SerializeField] private GameObject vfxGO;

        [Header("Collision Layers")]
        [SerializeField] private LayerMask interactLayers = ~0;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip bounceClip;
        [SerializeField] private AudioClip returnToOwnerClip;

        private Rigidbody rb;
        private Collider col;
        private Vector3 lastDirection = Vector3.right;
        private int countCollision;
        private VT_PlayerController currentOwner;
        private VT_AudioManager audioManager;

        bool IsAttachedToOwner => currentOwner != null && transform.parent == currentOwner.shootPos;

        void Awake()
        {
            audioManager = VT_AudioManager.Instance;

            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();

            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;

            rb.isKinematic = true;
            rb.detectCollisions = false;

            if (vfxGO != null) vfxGO.SetActive(false);
        }

        void FixedUpdate()
        {
            if (!gameObject.activeInHierarchy) return;

            if (IsAttachedToOwner || rb.isKinematic)
            {
                if (IsAttachedToOwner)
                {
                    transform.position = currentOwner.shootPos.position;
                    transform.rotation = currentOwner.shootPos.rotation;
                }

                if (!rb.isKinematic)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                return;
            }

            Vector3 v = rb.linearVelocity;

            if (v.sqrMagnitude < 0.0001f)
            {
                v = lastDirection * moveSpeed;
            }
            else
            {
                v = v.normalized * moveSpeed;
                lastDirection = v.normalized;
            }

            rb.linearVelocity = v;
        }

        void LateUpdate()
        {
            if (IsAttachedToOwner)
            {
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            int otherLayer = collision.gameObject.layer;

            if ((interactLayers.value & (1 << otherLayer)) == 0)
            {
                if (col != null && collision.collider != null)
                {
                    Physics.IgnoreCollision(col, collision.collider);
                }

                return;
            }

            if (countCollision < 0) countCollision = 0;

            VT_PlayerController hitPlayer = collision.gameObject.GetComponent<VT_PlayerController>();

            if (hitPlayer == null)
            {
                countCollision++;

                audioManager.PlaySfx(bounceClip);

                if (countCollision >= maxBounce)
                {
                    ReturnToOwner();
                }
            }
            else
            {
                if (hitPlayer == currentOwner)
                {
                    ReturnToOwner();
                }
                else
                {
                    TransferOwnership(hitPlayer);
                }
            }
        }

        public void Shoot(VT_PlayerController shooter, Vector3 direction)
        {
            if (vfxGO != null) vfxGO.SetActive(true);

            currentOwner = shooter;
            countCollision = 0;

            transform.SetParent(null);

            rb.isKinematic = false;
            rb.detectCollisions = true;

            direction.y = 0;
            direction.Normalize();
            lastDirection = direction;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.linearVelocity = direction * moveSpeed;
        }

        private void AttachToOwner()
        {
            if (currentOwner == null) return;

            rb.isKinematic = true;
            rb.detectCollisions = false;

            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            transform.SetParent(currentOwner.shootPos);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            if (vfxGO != null) vfxGO.SetActive(false);
            countCollision = 0;
        }

        private void ReturnToOwner()
        {
            if (currentOwner == null) return;

            AttachToOwner();
            currentOwner.audioManager.PlaySfx(returnToOwnerClip);
            currentOwner.SetBall(this);
            currentOwner.SetHasCore(true);
        }

        private void TransferOwnership(VT_PlayerController newOwner)
        {
            if (currentOwner != null)
            {
                currentOwner.SetHasCore(false);
                currentOwner.SetBall(null);
            }

            currentOwner = newOwner;
            currentOwner.SetHasCore(true);
            currentOwner.SetBall(this);

            AttachToOwner();
        }

        public void SetInitialOwner(VT_PlayerController owner)
        {
            currentOwner = owner;
            owner.SetHasCore(true);
            owner.SetBall(this);

            AttachToOwner();
        }
    }
}
