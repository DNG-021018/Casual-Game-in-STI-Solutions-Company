using System;
using System.Collections;
using Cinemachine;
using UnityEngine;

namespace CataFury
{
    public class CF_PlayerController : MonoBehaviour
    {
        private Animator animator;


        [Header("Projectile Spawn Point")]
        [SerializeField] private Transform projectileSpawnPoint;

        [Header("Projectile Settings")]
        [SerializeField] private float projectileDamage = 1f;

        [Header("Auto Shoot Settings")]
        [SerializeField] private float fireRate = 2f;
        [SerializeField] private float directionThreshold = 0.5f;

        [Header("Combo Attack Box")]
        [SerializeField] private CF_PlayerAttackBox comboAttackBox;

        [Header("Booster Settings")]
        [SerializeField] private float boosterFireRate = 6f;
        [SerializeField] private float boosterDuration = 4f;
        [SerializeField] private float boosterCooldown = 10f;
        [SerializeField] private AudioClip boosterActivateSfx;
        [SerializeField] private ParticleSystem boosterEffect;

        [Header("Combo Settings")]
        [SerializeField] private ParticleSystem comboNotify;
        [SerializeField] private ParticleSystem effectDuringCombo;
        [SerializeField] private int comboRequiredKills = 10;
        [SerializeField] private float comboDuration = 5f;
        [SerializeField] private AudioClip comboActivateSfx;

        [Header("Impulse")]
        [SerializeField] private CinemachineImpulseSource impulseSource;
        [SerializeField] private float impulseForce = 0.3f;
        [SerializeField] private float comboImpulseForce = 0.8f;


        private CF_AudioManager _audioManager;
        private CF_PoolManager _poolManager;

        private Color _projectileColor = Color.white;

        private PlayerDirection _currentDirection = PlayerDirection.Down;
        private float _fireTimer = 0f;
        private bool _isPlaying = false;

        private bool _isBoosted = false;
        private bool _boosterOnCooldown = false;
        private float _boosterCooldownRemaining = 0f;
        private Coroutine _boosterCoroutine;

        private int _killStreak = 0;
        private bool _isComboActive = false;
        private Coroutine _comboTimerCoroutine;


        public static event Action OnPlayerDead;
        public static event Action<int, int> OnKillStreakChanged;
        public static event Action<bool> OnComboStateChanged;
        public static event Action<bool> OnBoosterStateChanged;
        public static event Action<float, float> OnBoosterCooldownTick;


        public bool IsComboActive => _isComboActive;
        public int KillStreak => _killStreak;
        public float ComboDuration => comboDuration;
        public bool IsBoosted => _isBoosted;
        public bool BoosterReady => !_isBoosted && !_boosterOnCooldown;
        public float BoosterCooldown => boosterCooldown;


        void Awake() => Init();

        private void Init()
        {
            _audioManager = ServiceLocator.Get<CF_AudioManager>();
            _poolManager = ServiceLocator.Get<CF_PoolManager>();

            StopComboParticles();
            StopBoosterEffect();

            comboAttackBox?.SetAlwaysActive(false);
            RotatePlayer(PlayerDirection.Down);
        }

        void OnEnable() => CF_GameManager.OnGameStateChanged += HandleGameState;
        void OnDisable() => CF_GameManager.OnGameStateChanged -= HandleGameState;

        private void HandleGameState(GameState state)
        {
            _isPlaying = state == GameState.Play;
            if (!_isPlaying) _fireTimer = 0f;
        }


        void Update()
        {
            if (!_isPlaying) return;

            if (_boosterOnCooldown)
            {
                _boosterCooldownRemaining -= Time.deltaTime;
                OnBoosterCooldownTick?.Invoke(_boosterCooldownRemaining, boosterCooldown);

                if (_boosterCooldownRemaining <= 0f)
                {
                    _boosterOnCooldown = false;
                    _boosterCooldownRemaining = 0f;
                    OnBoosterCooldownTick?.Invoke(0f, boosterCooldown);
                }
            }

            _fireTimer -= Time.deltaTime;
            if (_fireTimer > 0f) return;

            float currentRate = _isBoosted ? boosterFireRate : fireRate;
            _fireTimer = 1f / currentRate;

            Transform target = GetNearestEnemyInDirection();
            if (target != null)
                Shoot(target);
        }

        private Transform GetNearestEnemyInDirection()
        {
            if (_poolManager == null) return null;

            Vector3 facingDir = transform.forward;
            facingDir.y = 0f;
            facingDir.Normalize();

            Transform nearest = null;
            float minDist = float.MaxValue;

            _poolManager.EnemyPool.ForEachActive(enemy =>
            {
                Vector3 toEnemy = enemy.transform.position - transform.position;
                toEnemy.y = 0f;

                if (Vector3.Dot(facingDir, toEnemy.normalized) < directionThreshold) return;

                float dist = toEnemy.magnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = enemy.transform;
                }
            });

            return nearest;
        }


        private void Shoot(Transform target)
        {
            if (animator != null)
            {
                animator.ResetTrigger(CF_SafetyKey.Animation.ANIM_TRIGGER_ATTACK);
                animator.SetTrigger(CF_SafetyKey.Animation.ANIM_TRIGGER_ATTACK);
            }

            SpawnProjectile(target);
            TriggerImpulse();
        }

        private void SpawnProjectile(Transform target)
        {
            if (_poolManager?.ProjectilePool == null) return;

            Vector3 spawnPos = projectileSpawnPoint != null
                ? projectileSpawnPoint.position
                : transform.position + Vector3.up * 1f;

            CF_Projectile proj = _poolManager.ProjectilePool.GetFromPool(spawnPos, Quaternion.identity);
            if (proj == null) return;

            proj.SetColor(_projectileColor);

            proj.Launch(
                target: target,
                direction: transform.forward,
                damage: projectileDamage,
                isPiercing: _isBoosted
            );
        }


        public void SetAnimator(Animator newAnimator) => animator = newAnimator;

        public void SetProjectileColor(Color color)
        {
            _projectileColor = color;
        }

        public void MovePlayer(PlayerDirection direction)
        {
            _currentDirection = direction;
            RotatePlayer(direction);
        }

        public void ResetPlayer()
        {
            animator?.ResetTrigger(CF_SafetyKey.Animation.ANIM_TRIGGER_ATTACK);
            ResetCombo();
            ResetBooster();
            RotatePlayer(PlayerDirection.Down);
            _currentDirection = PlayerDirection.Down;
            _fireTimer = 0f;
        }


        public void ActivateBooster()
        {
            if (_isBoosted || _boosterOnCooldown) return;
            if (_boosterCoroutine != null) StopCoroutine(_boosterCoroutine);
            _boosterCoroutine = StartCoroutine(BoosterRoutine());
        }

        private IEnumerator BoosterRoutine()
        {
            _isBoosted = true;
            _fireTimer = 0f;

            _audioManager?.PlaySfx(boosterActivateSfx);
            boosterEffect?.Play(true);
            OnBoosterStateChanged?.Invoke(true);

            yield return new WaitForSeconds(boosterDuration);

            _isBoosted = false;
            StopBoosterEffect();
            OnBoosterStateChanged?.Invoke(false);

            _boosterOnCooldown = true;
            _boosterCooldownRemaining = boosterCooldown;
        }

        private void ResetBooster()
        {
            if (_boosterCoroutine != null) { StopCoroutine(_boosterCoroutine); _boosterCoroutine = null; }
            _isBoosted = false;
            _boosterOnCooldown = false;
            _boosterCooldownRemaining = 0f;
            StopBoosterEffect();
            OnBoosterStateChanged?.Invoke(false);
        }

        private void StopBoosterEffect()
            => boosterEffect?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);


        public void RegisterKill()
        {
            if (_isComboActive) return;
            _killStreak++;
            OnKillStreakChanged?.Invoke(_killStreak, comboRequiredKills);
            if (_killStreak >= comboRequiredKills) ActivateCombo();
        }

        private void ActivateCombo()
        {
            _isComboActive = true;
            _killStreak = 0;

            comboAttackBox?.SetAlwaysActive(true);
            _audioManager?.PlaySfx(comboActivateSfx);
            OnComboStateChanged?.Invoke(true);

            comboNotify?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            comboNotify?.Play(true);
            effectDuringCombo?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            effectDuringCombo?.Play(true);

            if (_comboTimerCoroutine != null) StopCoroutine(_comboTimerCoroutine);
            _comboTimerCoroutine = StartCoroutine(ComboTimer());
        }

        private IEnumerator ComboTimer()
        {
            yield return new WaitForSeconds(comboDuration);
            DeactivateCombo();
        }

        private void DeactivateCombo()
        {
            _isComboActive = false;
            comboAttackBox?.SetAlwaysActive(false);
            StopComboParticles();
            OnComboStateChanged?.Invoke(false);
        }

        private void ResetCombo()
        {
            if (_comboTimerCoroutine != null) { StopCoroutine(_comboTimerCoroutine); _comboTimerCoroutine = null; }
            _killStreak = 0;
            _isComboActive = false;
            comboAttackBox?.SetAlwaysActive(false);
            StopComboParticles();
            OnComboStateChanged?.Invoke(false);
            OnKillStreakChanged?.Invoke(0, comboRequiredKills);
        }

        private void StopComboParticles()
        {
            comboNotify?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            effectDuringCombo?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }


        public void TriggerImpulse(float force = -1f)
        {
            if (impulseSource == null) return;
            impulseSource.GenerateImpulse(force < 0 ? impulseForce : force);
        }

        private void HandleDeath()
        {
            OnPlayerDead?.Invoke();
            TriggerImpulse(comboImpulseForce);
            CF_GameManager.Instance.FinishGame(GameState.Lose);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(CF_SafetyKey.Tag.TAG_ENEMY))
                HandleDeath();
        }

        private void RotatePlayer(PlayerDirection direction)
        {
            transform.rotation = Quaternion.Euler(direction switch
            {
                PlayerDirection.Left => new Vector3(0, -90, 0),
                PlayerDirection.Right => new Vector3(0, 90, 0),
                PlayerDirection.Up => new Vector3(0, 0, 0),
                PlayerDirection.Down => new Vector3(0, 180, 0),
                _ => Vector3.zero
            });
        }
    }
}