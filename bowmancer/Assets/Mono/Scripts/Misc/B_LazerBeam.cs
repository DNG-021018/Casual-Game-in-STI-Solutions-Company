using UnityEngine;
using System.Collections.Generic;

namespace Bowmancer
{
    public class B_LazerBeam : MonoBehaviour
    {
        [SerializeField] float Damage = 10f;
        [SerializeField] float DamageInterval = 1f;
        [SerializeField] Transform shootPoint;
        [SerializeField] float maxLaserLength = 100f;
        [SerializeField] LayerMask blockLayer;
        [SerializeField] LineRenderer laserLineRenderer;
        [SerializeField] ParticleSystem hitEffectParticle;
        [SerializeField] float damageRadiusVisual = 0.5f;

        private float damageCountdown = 0f;
        private List<B_PlayerController> hitPlayers = new List<B_PlayerController>();
        private bool isActive = false;
        private Vector3 laserEndPoint = Vector3.zero;

        void OnEnable()
        {
            StartLaser();
        }

        void OnDisable()
        {
            StopLaser();
        }

        public void StartLaser()
        {
            isActive = true;
            hitPlayers.Clear();
            damageCountdown = 0f;
            if (laserLineRenderer != null)
                laserLineRenderer.enabled = true;
        }

        public void StopLaser()
        {
            isActive = false;
            hitPlayers.Clear();
            damageCountdown = 0f;
            if (laserLineRenderer != null)
                laserLineRenderer.enabled = false;
            if (hitEffectParticle != null)
                hitEffectParticle.Stop();
        }

        void Update()
        {
            if (!isActive || shootPoint == null)
                return;

            // Update damage countdown
            damageCountdown -= Time.deltaTime;

            // Raycast thẳng từ shootPoint
            Vector3 laserDirection = shootPoint.forward;
            Vector3 laserStart = shootPoint.position;
            float laserLength = maxLaserLength;
            laserEndPoint = laserStart + laserDirection * laserLength;

            // Check tất cả các collider trong đường laser
            RaycastHit[] hits = Physics.RaycastAll(laserStart, laserDirection, maxLaserLength);

            // Sort hits theo distance
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            // Clear list trước, rồi rebuild lại từ raycast hits
            List<B_PlayerController> currentFramePlayers = new List<B_PlayerController>();

            for (int i = 0; i < hits.Length; i++)
            {
                // Nếu va chạm với block layer, laser dừng lại
                if (((1 << hits[i].collider.gameObject.layer) & blockLayer) != 0)
                {
                    laserLength = hits[i].distance;
                    laserEndPoint = hits[i].point;
                    break;
                }

                // Nếu va chạm với Player, gây damage nhưng laser vẫn đi qua
                if (hits[i].collider.CompareTag(B_SafetyKey.TAG_PLAYER))
                {
                    B_PlayerController player = hits[i].collider.GetComponent<B_PlayerController>();
                    if (player != null && !currentFramePlayers.Contains(player))
                    {
                        currentFramePlayers.Add(player);
                        if (!hitPlayers.Contains(player))
                        {
                            hitPlayers.Add(player);
                        }
                    }
                }
            }

            // Remove players không còn trong laser
            for (int i = hitPlayers.Count - 1; i >= 0; i--)
            {
                if (!currentFramePlayers.Contains(hitPlayers[i]))
                {
                    hitPlayers.RemoveAt(i);
                }
            }

            // Gây damage cho các player trong laser
            if (damageCountdown <= 0f && hitPlayers.Count > 0)
            {
                foreach (var player in hitPlayers)
                {
                    if (player != null)
                    {
                        player.TakeDamage(Damage);
                    }
                }
                damageCountdown = DamageInterval;
            }

            // Vẽ laser line
            if (laserLineRenderer != null)
            {
                laserLineRenderer.SetPosition(0, laserStart);
                laserLineRenderer.SetPosition(1, laserStart + laserDirection * laserLength);
            }

            // Play particle effect ở điểm va chạm
            if (hitEffectParticle != null)
            {
                hitEffectParticle.transform.position = laserEndPoint;
                if (!hitEffectParticle.isPlaying)
                {
                    hitEffectParticle.Play();
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            // Khi player rời khỏi laser, xoá khỏi list
            if (other.CompareTag(B_SafetyKey.TAG_PLAYER))
            {
                B_PlayerController player = other.GetComponent<B_PlayerController>();
                if (player != null)
                {
                    hitPlayers.Remove(player);
                }
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (shootPoint == null)
                return;

            // Vẽ đường laser
            Vector3 laserStart = shootPoint.position;
            Vector3 laserDirection = shootPoint.forward;
            Vector3 laserEnd = laserStart + laserDirection * maxLaserLength;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(laserStart, laserEnd);

            // Vẽ vùng damage tại điểm kết thúc (sphere)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(laserEnd, damageRadiusVisual);

            // Vẽ điểm bắt đầu
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(laserStart, 0.2f);
        }

        void OnDrawGizmosSelected()
        {
            if (shootPoint == null)
                return;

            // Khi object được chọn, vẽ các gizmos với màu sáng hơn
            Vector3 laserStart = shootPoint.position;
            Vector3 laserDirection = shootPoint.forward;
            Vector3 laserEnd = laserStart + laserDirection * maxLaserLength;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(laserStart, laserEnd);

            // Vẽ vùng damage
            Gizmos.color = new Color(1f, 0.5f, 0.5f, 1f);
            Gizmos.DrawWireSphere(laserEnd, damageRadiusVisual);

            // Vẽ điểm bắt đầu
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(laserStart, 0.2f);
        }
#endif
    }
}