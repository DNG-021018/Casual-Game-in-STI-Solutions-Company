using System.Collections;
using UnityEngine;

namespace Bowmancer
{
    public abstract class B_AEntity : MonoBehaviour, B_IDamage
    {
        [Header("Settings")]
        [SerializeField] protected B_EntitySetting _entitySetting;
        [SerializeField] protected B_Healthbar _healthbar;
        [SerializeField] protected SkinnedMeshRenderer[] meshRenderer;
        protected Color baseColor = Color.white;
        [SerializeField] protected Color hitColor = Color.red;

        [Space(10)]

        [Header("Floating Text")]
        [SerializeField] protected Color _damageTextColor = Color.red;
        protected B_FloatingTextPool _floatingText;
        protected B_PoolManager _poolManager;

        public float MoveSpeed { get; set; }
        public float RotationSpeed { get; protected set; }
        public float Health { get; set; }
        public float AttackPower { get; set; }
        public float MaxHealth { get; protected set; }

        protected float Gravity { get; set; }
        protected float GravityMultiplier { get; set; }

        protected AudioClip MoveClip { get; set; }
        protected AudioClip AttackClip { get; set; }
        protected AudioClip HitClip { get; set; }
        protected AudioClip DieClip { get; set; }
        protected B_AudioManager _audioManager;

        protected virtual void Awake()
        {
            InitStats();
            _audioManager = B_AudioManager.Instance;
            _poolManager = B_PoolManager.Instance;
            _floatingText = _poolManager.FloatingTextPool;
        }

        protected virtual void InitStats()
        {
            if (_entitySetting == null)
            {
                return;
            }

            EntityStats stats = _entitySetting.GetStats();

            MoveSpeed = stats.MoveSpeed;
            RotationSpeed = stats.RotationSpeed;
            Health = stats.Health;
            MaxHealth = stats.Health;
            AttackPower = stats.AttackPower;

            Gravity = stats.Gravity;
            GravityMultiplier = stats.GravityMultiplier;

            MoveClip = stats.MoveClip;
            AttackClip = stats.AttackClip;
            HitClip = stats.HitClip;
            DieClip = stats.DieClip;

            _healthbar = GetComponentInChildren<B_Healthbar>();
            _healthbar.Init(Health);

            meshRenderer = GetComponentsInChildren<SkinnedMeshRenderer>();
        }

        protected abstract void HandleMoving();
        protected abstract void HandleDie();
        protected abstract void ApplyGravity();
        protected abstract void ApplyMovement(Vector3 moveDir);

        public virtual void TakeDamage(float damage)
        {
            if (Health <= 0f) return;

            Health -= damage;
            if (HitClip != null)
            {
                B_AudioManager.Instance.PlaySfx(HitClip);
            }
            _healthbar.SetHealth(Health);
            StartCoroutine(FlashHitColor());
            B_FloatingText floatingText = _floatingText.Get("DamageText", transform.position + Vector3.up * 2f, Quaternion.identity);
            floatingText.InitPool(_floatingText);
            floatingText.ShowFloatingText(damage.ToString(), transform, _damageTextColor);

            if (Health <= 0f)
            {
                HandleDie();
            }
        }

        public void SetMaxHealth(float newMaxHealth)
        {
            MaxHealth = newMaxHealth;
            if (_healthbar != null)
            {
                _healthbar.Init(MaxHealth);
                _healthbar.SetHealth(Health);
            }
        }

        public void SetAttackPower(float newAttackPower)
        {
            AttackPower = newAttackPower;
        }

        public void SetMoveSpeed(float newMoveSpeed)
        {
            MoveSpeed = newMoveSpeed;
        }

        public void Heal(float amount)
        {
            Health += amount;
            if (_healthbar != null)
            {
                _healthbar.SetHealth(Health);
            }
        }

        protected IEnumerator FlashHitColor()
        {
            if (meshRenderer == null) yield break;

            foreach (var renderer in meshRenderer)
            {
                renderer.material.color = hitColor;
            }

            yield return new WaitForSeconds(0.1f);

            foreach (var renderer in meshRenderer)
            {
                renderer.material.color = baseColor;
            }
        }

        public float GetBaseHealth() => _entitySetting != null ? _entitySetting.GetStats().Health : 0f;
        public float GetCurrentHealth() => Health;
        public float GetMaxHealth() => MaxHealth;
        public Transform GetTransform() => transform;
    }
}
