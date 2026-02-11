using System.Collections;
using UnityEngine;

namespace CubeSokoban
{
    public class CS_Box : MonoBehaviour
    {
        [SerializeField] private Material normalMaterial;
        [SerializeField] private Material hightLightMaterial;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float raycastLength;
        [SerializeField] private AudioClip BoxMove;
        [SerializeField] private AudioClip HitGoal;

        private bool _isMoving;
        private int goalsCount = 0;

        private MeshRenderer _meshRenderer;
        private CS_AudioManager _audioManager;


        void Awake()
        {
            _audioManager = CS_AudioManager.Instance;
            _meshRenderer = GetComponentInChildren<MeshRenderer>();
        }

        public bool TryToMove(Vector2 startPos, Vector2 endPos, float speed)
        {
            if (_isMoving) return false;

            Vector2 swipeDelta = endPos - startPos;

            _ = Vector3.zero;
            Vector3 moveDir;

            if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
            {
                moveDir = swipeDelta.x > 0 ? Vector3.right : Vector3.left;
            }
            else
            {
                moveDir = swipeDelta.y > 0 ? Vector3.forward : Vector3.back;
            }

            Vector3 nextPos = transform.position + moveDir;

            if (!IsGroundAtPosition(nextPos)) return false;

            StartCoroutine(Move(speed, nextPos));

            return true;
        }

        private bool IsGroundAtPosition(Vector3 pos)
        {
            if (!Physics.Raycast(pos + Vector3.up, Vector3.down, raycastLength, _groundLayer))
                return false;

            Collider[] colliders = Physics.OverlapSphere(pos, 0.3f, _groundLayer);
            foreach (Collider col in colliders)
            {
                if (col.gameObject != gameObject && col.CompareTag("Box"))
                    return false;
            }
            return true;
        }

        private IEnumerator Move(float speed, Vector3 target)
        {
            _audioManager.PlaySfx(BoxMove);
            _isMoving = true;
            while (Vector3.Distance(transform.position, target) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
                yield return null;
            }
            transform.position = target;
            _isMoving = false;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Goal"))
            {
                if (goalsCount == 0)
                {
                    _audioManager.PlaySfx(HitGoal);
                    _meshRenderer.material = hightLightMaterial;
                    if (CS_LevelManager.Instance != null)
                    {
                        CS_LevelManager.Instance.OnGoalButtonStateChanged(true);
                    }
                }
                goalsCount++;
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Goal"))
            {
                goalsCount--;
                if (goalsCount <= 0)
                {
                    _meshRenderer.material = normalMaterial;
                    if (CS_LevelManager.Instance != null)
                    {
                        CS_LevelManager.Instance.OnGoalButtonStateChanged(false);
                    }
                }
            }
        }

        // private void OnDrawGizmos()
        // {
        //     Vector3[] directions = { Vector3.right, Vector3.left, Vector3.forward, Vector3.back };
        //     Color[] colors = { Color.red, Color.green, Color.blue, Color.yellow };

        //     for (int i = 0; i < directions.Length; i++)
        //     {
        //         Vector3 nextPos = transform.position + directions[i];
        //         Vector3 rayStart = nextPos + Vector3.up;
        //         Vector3 rayEnd = nextPos + Vector3.down * 2;

        //         Gizmos.color = colors[i];
        //         Gizmos.DrawLine(rayStart, rayEnd);

        //         if (Physics.Raycast(rayStart, Vector3.down, raycastLength, _groundLayer))
        //         {
        //             Gizmos.color = colors[i];
        //             Gizmos.DrawSphere(nextPos, 0.1f);
        //         }
        //     }
        // }
    }
}
