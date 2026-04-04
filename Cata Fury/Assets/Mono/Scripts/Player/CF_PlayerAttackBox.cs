using System.Collections.Generic;
using _Workspace._Scripts.Core.UtilityCore.PoolingCore;
using UnityEngine;

namespace CataFury
{
    [RequireComponent(typeof(BoxCollider))]
    public class CF_PlayerAttackBox : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [SerializeField] private LayerMask targetLayer;

        [Header("Stay Hit Cooldown")]
        [SerializeField] private float stayHitInterval = 0.2f;

        private BoxCollider _box;
        private bool _alwaysActive = false;

        private readonly Dictionary<Collider, float> _lastHitTime = new();

        private Pooler<CF_FloatingText> _floatingTextPool;
        private CF_CurrencyManager _currencyManager;
        private CF_ScoreManager _scoreManager;
        private CF_PlayerController _playerController;

        private void Awake()
        {
            _box = GetComponent<BoxCollider>();
            _box.isTrigger = true;

            _floatingTextPool = ServiceLocator.Get<CF_PoolManager>().FloatingTextPool;
            _currencyManager = ServiceLocator.Get<CF_CurrencyManager>();
            _scoreManager = ServiceLocator.Get<CF_ScoreManager>();
            _playerController = GetComponentInParent<CF_PlayerController>();
        }

        public void SetAlwaysActive(bool active)
        {
            _alwaysActive = active;
            if (!active) _lastHitTime.Clear();
        }


        private void OnTriggerEnter(Collider other)
        {
            if (!_alwaysActive) return;
            if (((1 << other.gameObject.layer) & targetLayer) == 0) return;
            if (!other.TryGetComponent(out IDamageable damageable)) return;

            _lastHitTime[other] = Time.time;
            ProcessHit(damageable, other, other.bounds.center, 9999f);
        }


        private void OnTriggerStay(Collider other)
        {
            if (!_alwaysActive) return;
            if (((1 << other.gameObject.layer) & targetLayer) == 0) return;
            if (!other.TryGetComponent(out IDamageable damageable)) return;

            if (_lastHitTime.TryGetValue(other, out float lastTime))
            {
                if (Time.time - lastTime < stayHitInterval) return;
            }

            _lastHitTime[other] = Time.time;
            ProcessHit(damageable, other, other.bounds.center, 9999f);
        }


        private void OnTriggerExit(Collider other)
        {
            _lastHitTime.Remove(other);
        }

        private void ProcessHit(IDamageable damageable, Collider col, Vector3 hitPoint, float damage)
        {
            bool killed = damageable.ApplyDamage(damage, hitPoint);

            _playerController?.TriggerImpulse();

            if (killed)
            {
                _floatingTextPool?
                    .Get("FloatingText", col.transform.position + Vector3.up * 1.5f, Quaternion.identity)
                    .ShowFloatingText("+1", col.transform);

                _currencyManager?.AddCoins(1);
                _scoreManager?.AddScore(1);
                _playerController?.RegisterKill();

                _lastHitTime.Remove(col);
            }
            else
            {
                _floatingTextPool?
                    .Get("FloatingHealth", col.transform.position + Vector3.up * 1.8f, Quaternion.identity)
                    .ShowFloatingText("-1", col.transform);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_box == null) _box = GetComponent<BoxCollider>();
            if (_box == null) return;

            Vector3 center = transform.TransformPoint(_box.center);
            Vector3 size = Vector3.Scale(_box.size, transform.lossyScale);

            Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);

            Gizmos.color = new Color(1f, 0.3f, 0f, 0.25f);
            Gizmos.DrawCube(Vector3.zero, size);

            Gizmos.color = new Color(1f, 0.3f, 0f, 1f);
            Gizmos.DrawWireCube(Vector3.zero, size);

            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
