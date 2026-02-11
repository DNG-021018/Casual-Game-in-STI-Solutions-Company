using System.Collections.Generic;
using UnityEngine;

namespace Bowmancer
{
    public class B_BulletModifierSystem : MonoBehaviour
    {
        [SerializeField] AudioClip shootSound;

        private B_PlayerRef _playerRef;
        private List<IBulletModifier> _modifiers = new List<IBulletModifier>();

        private BulletStats _currentStats = new BulletStats();
        private B_AudioManager _audioManager;

        private void Awake()
        {
            _audioManager = B_AudioManager.Instance;
        }

        public void Initialize(B_PlayerRef playerRef)
        {
            _playerRef = playerRef;
            ResetStats();
        }

        public void AddModifier(IBulletModifier modifier)
        {
            if (!_modifiers.Contains(modifier))
            {
                _modifiers.Add(modifier);
                RecalculateStats();
            }
        }

        public void RemoveModifier(IBulletModifier modifier)
        {
            if (_modifiers.Remove(modifier))
            {
                RecalculateStats();
            }
        }

        public void ClearAllModifiers()
        {
            _modifiers.Clear();
            ResetStats();
        }

        private void RecalculateStats()
        {
            ResetStats();

            foreach (var modifier in _modifiers)
            {
                modifier.ModifyStats(ref _currentStats);
            }
        }

        private void ResetStats()
        {
            if (_playerRef == null || _playerRef.PlayerController == null)
            {
                _currentStats = new BulletStats
                {
                    damage = 10f,
                    speed = 50f,
                    projectileCount = 1,
                    critChance = 0f,
                    critMultiplier = 2f,
                    pierceCount = 0,
                    spreadAngle = 0f,
                    homingStrength = 0f
                };
                return;
            }

            float baseGunDamage = 2f;

            _currentStats = new BulletStats
            {
                damage = baseGunDamage,
                speed = 50f,
                projectileCount = 1,
                critChance = 0f,
                critMultiplier = 2f,
                pierceCount = 0,
                spreadAngle = 0f,
                homingStrength = 0f
            };
        }

        public void OnShootBullet(B_Bullet bullet, Transform target)
        {
            ApplyStatsTooBullet(bullet);

            foreach (var modifier in _modifiers)
            {
                modifier.OnBulletFired(bullet, target);
            }
        }

        public void ShootProjectiles(Transform shootPoint, Transform target, B_BulletPool bulletPool, B_VFXPool vfxPool)
        {
            int count = _currentStats.projectileCount;
            float spread = _currentStats.spreadAngle;

            if (count == 1)
            {
                ShootSingleBullet(shootPoint, target, bulletPool, vfxPool, 0f);
            }
            else
            {
                float startAngle = -(spread * (count - 1)) / 2f;

                for (int i = 0; i < count; i++)
                {
                    float angle = startAngle + (spread * i);
                    ShootSingleBullet(shootPoint, target, bulletPool, vfxPool, angle);
                }
            }
        }

        private void ShootSingleBullet(Transform shootPoint, Transform target, B_BulletPool bulletPool, B_VFXPool vfxPool, float angleOffset)
        {
            B_Bullet bullet = bulletPool.GetFromPool(shootPoint.position, shootPoint.rotation);
            if (bullet == null) return;
            _audioManager.PlaySfx(shootSound, 0.3f);
            bullet.InitPool(bulletPool);
            bullet.InitHitEffectPool(vfxPool);

            Vector3 direction = (target.position - shootPoint.position).normalized;
            if (angleOffset != 0f)
            {
                Quaternion rotation = Quaternion.Euler(0, angleOffset, 0);
                direction = rotation * direction;
            }

            float damage = CalculateDamage();

            if (angleOffset != 0f && _currentStats.sideProjectileDamageMultiplier > 0f)
            {
                damage *= _currentStats.sideProjectileDamageMultiplier;
            }

            bullet.InitializedWithDirection(damage, shootPoint, direction);

            OnShootBullet(bullet, target);
        }

        private void ApplyStatsTooBullet(B_Bullet bullet)
        {
        }

        private float CalculateDamage()
        {
            float damage = _currentStats.damage + _playerRef.PlayerController.AttackPower;

            if (Random.value < _currentStats.critChance)
            {
                damage *= _currentStats.critMultiplier;
            }

            return damage;
        }

        public BulletStats CurrentStats => _currentStats;
        public List<IBulletModifier> ActiveModifiers => new List<IBulletModifier>(_modifiers);
    }

    [System.Serializable]
    public struct BulletStats
    {
        public float damage;
        public float speed;
        public int projectileCount;
        public float critChance;
        public float critMultiplier;
        public int pierceCount;
        public float spreadAngle;
        public float homingStrength;
        public float sideProjectileDamageMultiplier;
        public Color critColor;
    }

    public interface IBulletModifier
    {
        void ModifyStats(ref BulletStats stats);

        void OnBulletFired(B_Bullet bullet, Transform target);
    }
}