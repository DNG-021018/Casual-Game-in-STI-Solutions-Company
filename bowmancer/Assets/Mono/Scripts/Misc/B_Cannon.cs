using System.Collections.Generic;
using _Workspace._Scripts.Core.UtilityCore.PoolingCore;
using UnityEngine;
using UnityEngine.UI;

namespace Bowmancer
{
    public class B_Cannon : MonoBehaviour
    {
        [Header("Cannon Settings")]
        [SerializeField] private CannonType cannonType = CannonType.Straight;
        [SerializeField] private Transform firePoint;
        [SerializeField] private AudioClip fireSound;

        [Header("Firing Settings")]
        [SerializeField] private float fireRate = 1f;
        [SerializeField] private float bulletSpeed = 10f;
        [SerializeField] private float bulletLifetime = 5f;

        [Header("Delay Settings")]
        [SerializeField] private float fireDelay = 1f;
        [SerializeField] private bool useFireDelay = true;

        [Header("Rotation Settings (for Rotating type)")]
        [SerializeField] private float rotationSpeed = 30f;
        [SerializeField] private Vector3 rotationAxis = Vector3.up;

        [Header("Path Detection")]
        [SerializeField] private int maxBounces = 3;
        [SerializeField] private float maxPathDistance = 50f;
        [SerializeField] private LayerMask bounceLayerMask;
        [SerializeField] private float raycastRadius = 0.1f;

        [Header("UI Indicator Settings")]
        [SerializeField] private Image indicatorImage;
        [SerializeField] private bool showIndicator = true;
        [SerializeField] private Color indicatorChargeColor = new Color(1f, 0.5f, 0f, 0.8f);
        [Tooltip("Màu indicator khi sắp bắn")]
        [SerializeField] private Color indicatorReadyColor = new Color(1f, 0f, 0f, 1f);

        [Header("Gizmos Settings")]
        [SerializeField] private bool showPath = true;
        [SerializeField] private Color pathColor = Color.yellow;
        [SerializeField] private Color bouncePointColor = Color.red;
        [SerializeField] private float bouncePointSize = 0.2f;

        private B_PoolManager _poolManager;
        private Pooler<B_CannonBullet> bulletPool;
        private Pooler<B_CannonBulletVFX> bulletVFXPool;

        private float nextFireTime;
        private List<Vector3> detectedPath = new List<Vector3>();
        private List<Vector3> bouncePoints = new List<Vector3>();

        private bool isPreparingToFire = false;
        private float firePreparationStartTime;
        private Color originalIndicatorColor;

        private B_AudioManager _audioManager;

        public enum CannonType
        {
            Straight,
            Rotating
        }

        void Awake()
        {
            _poolManager = B_PoolManager.Instance;
            bulletPool = _poolManager.CannonPool;
            bulletVFXPool = _poolManager.CannonBulletPool;

            _audioManager = B_AudioManager.Instance;
        }

        void Start()
        {
            if (firePoint == null)
                firePoint = transform;

            if (indicatorImage != null)
            {
                if (indicatorImage.type != Image.Type.Filled)
                {
                    indicatorImage.type = Image.Type.Filled;
                }

                originalIndicatorColor = indicatorImage.color;

                HideIndicator();
            }
        }

        void Update()
        {
            DetectPath();

            if (cannonType == CannonType.Rotating)
            {
                transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
            }

            if (Time.time >= nextFireTime && !isPreparingToFire)
            {
                if (useFireDelay)
                {
                    StartFirePreparation();
                }
                else
                {
                    Fire();
                    nextFireTime = Time.time + 1f / fireRate;
                }
            }

            if (isPreparingToFire)
            {
                UpdateIndicatorFill();

                float elapsedTime = Time.time - firePreparationStartTime;
                if (elapsedTime >= fireDelay)
                {
                    Fire();
                    HideIndicator();
                    isPreparingToFire = false;
                    nextFireTime = Time.time + 1f / fireRate;
                }
            }
        }

        void StartFirePreparation()
        {
            isPreparingToFire = true;
            firePreparationStartTime = Time.time;

            if (showIndicator && indicatorImage != null)
            {
                ShowIndicator();
            }
        }

        void ShowIndicator()
        {
            if (indicatorImage == null)
                return;

            indicatorImage.gameObject.SetActive(true);
            indicatorImage.fillAmount = 0f;
            indicatorImage.color = indicatorChargeColor;
        }

        void UpdateIndicatorFill()
        {
            if (indicatorImage == null || !isPreparingToFire)
                return;

            float progress = (Time.time - firePreparationStartTime) / fireDelay;
            progress = Mathf.Clamp01(progress);

            indicatorImage.fillAmount = progress;

            if (progress >= 0.8f)
            {
                indicatorImage.color = Color.Lerp(indicatorChargeColor, indicatorReadyColor, (progress - 0.8f) / 0.2f);
            }
            else
            {
                indicatorImage.color = indicatorChargeColor;
            }
        }

        void HideIndicator()
        {
            if (indicatorImage == null)
                return;

            indicatorImage.fillAmount = 0f;
            indicatorImage.color = originalIndicatorColor;
            indicatorImage.gameObject.SetActive(false);
        }

        void DetectPath()
        {
            detectedPath.Clear();
            bouncePoints.Clear();

            Vector3 currentPosition = firePoint.position;
            Vector3 currentDirection = firePoint.forward;
            float remainingDistance = maxPathDistance;

            detectedPath.Add(currentPosition);

            for (int bounce = 0; bounce <= maxBounces; bounce++)
            {
                RaycastHit hit;

                if (Physics.SphereCast(currentPosition, raycastRadius, currentDirection,
                    out hit, remainingDistance, bounceLayerMask))
                {
                    detectedPath.Add(hit.point);
                    bouncePoints.Add(hit.point);

                    if (bounce >= maxBounces)
                        break;

                    currentDirection = Vector3.Reflect(currentDirection, hit.normal);
                    currentPosition = hit.point + hit.normal * 0.01f;

                    remainingDistance -= hit.distance;

                    if (remainingDistance <= 0)
                        break;
                }
                else
                {
                    detectedPath.Add(currentPosition + currentDirection * remainingDistance);
                    break;
                }
            }
        }

        void Fire()
        {
            if (detectedPath.Count < 2)
                return;

            B_CannonBullet bullet = bulletPool.GetRandom(firePoint.position, Quaternion.identity);
            bullet.InitPool(bulletPool);
            bullet.InitVFXPool(bulletVFXPool);
            // _audioManager.PlaySfx(fireSound, 0.2f);
            B_CannonBullet bulletScript = bullet.GetComponent<B_CannonBullet>();
            bulletScript.SetPath(new List<Vector3>(detectedPath), bulletSpeed, bulletLifetime);
        }

        void OnDrawGizmos()
        {
            if (!showPath || detectedPath.Count < 2)
                return;

            Gizmos.color = pathColor;
            for (int i = 0; i < detectedPath.Count - 1; i++)
            {
                Gizmos.DrawLine(detectedPath[i], detectedPath[i + 1]);
            }

            Gizmos.color = bouncePointColor;
            foreach (Vector3 point in bouncePoints)
            {
                Gizmos.DrawSphere(point, bouncePointSize);
            }

            if (firePoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(firePoint.position, firePoint.forward * 2f);
            }
        }

        void OnDisable()
        {
            HideIndicator();
            isPreparingToFire = false;
        }

        void OnDestroy()
        {
            HideIndicator();
        }
    }
}
