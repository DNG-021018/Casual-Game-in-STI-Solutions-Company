using UnityEngine;

namespace Bowmancer
{
    [RequireComponent(typeof(Rigidbody))]
    public class B_PushableBall : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool useKinematic = false;
        [SerializeField] private float pushForceMultiplier = 5f;
        [SerializeField] private float maxPushForce = 20f;

        [Header("Physics Settings")]
        [SerializeField] private float drag = 0.5f;
        [SerializeField] private float angularDrag = 0.5f;
        [SerializeField] private float mass = 1f;

        [Header("Kinematic Settings")]
        [SerializeField] private float kinematicMoveSpeed = 5f;
        [SerializeField] private float velocityDecayRate = 2f;

        private Rigidbody rb;
        private Vector3 currentVelocity = Vector3.zero;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            SetupRigidbody();
        }

        void SetupRigidbody()
        {
            if (rb != null)
            {
                rb.isKinematic = useKinematic;
                rb.mass = mass;
                rb.drag = drag;
                rb.angularDrag = angularDrag;
                rb.useGravity = !useKinematic;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
        }

        void FixedUpdate()
        {
            if (useKinematic && currentVelocity.magnitude > 0.01f)
            {
                Vector3 newPosition = rb.position + currentVelocity * Time.fixedDeltaTime;
                rb.MovePosition(newPosition);

                currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, velocityDecayRate * Time.fixedDeltaTime);
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.CompareTag(B_SafetyKey.TAG_PLAYER))
            {
                return;
            }

            PushBall(collision);
        }

        void OnCollisionStay(Collision collision)
        {
            if (!collision.gameObject.CompareTag(B_SafetyKey.TAG_PLAYER))
            {
                return;
            }

            PushBall(collision);
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(B_SafetyKey.TAG_PLAYER))
            {
                ResetVelocity();
            }
        }

        void PushBall(Collision collision)
        {
            if (rb == null) return;

            Vector3 pushDirection = (transform.position - collision.transform.position).normalized;

            pushDirection.y = 0;
            pushDirection.Normalize();

            if (useKinematic)
            {
                float pushStrength = collision.relativeVelocity.magnitude * pushForceMultiplier * 0.1f;
                pushStrength = Mathf.Clamp(pushStrength, 0, maxPushForce * 0.1f);

                currentVelocity = pushDirection * pushStrength * kinematicMoveSpeed;
            }
            else
            {
                float pushForce = collision.relativeVelocity.magnitude * pushForceMultiplier;
                pushForce = Mathf.Clamp(pushForce, 0, maxPushForce);

                rb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
            }
        }

        public void ResetVelocity()
        {
            currentVelocity = Vector3.zero;
            if (rb != null && !useKinematic)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = useKinematic ? Color.yellow : Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);

            if (Application.isPlaying && currentVelocity.magnitude > 0.1f)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, currentVelocity.normalized * 2f);
            }
        }
    }
}
