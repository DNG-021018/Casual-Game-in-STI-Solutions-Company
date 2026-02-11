using UnityEngine;
using DG.Tweening;

namespace bJakGZQ3_Outer_World
{
    public enum ItemType
    {
        FOOD = 0,
        ROCKET = 1,
        AIDKIT = 2,
        DIAMOND = 3,
        GUN = 4,
    }

    public class bJakGZQ3_ItemPickup : MonoBehaviour
    {
        [Header("Item Info")]
        [SerializeField] bJakGZQ3_ItemConfig itemConfig;

        [Header("Spawn Pop Settings")]
        [SerializeField] float popScaleFrom = 0.3f;
        [SerializeField] float popDuration = 0.28f;
        [SerializeField] Ease popEase = Ease.OutBack;

        [Header("Spin Settings")]
        [SerializeField] float spinSpeedDegPerSec = 90f;

        [Header("Hover / Bob Settings")]
        [SerializeField] float bobUpOffset = 0.12f;
        [SerializeField] float bobDownOffset = 0.08f;
        [SerializeField] float bobDuration = 1.1f;
        [SerializeField] Ease bobEase = Ease.InOutSine;

        bJakGZQ3_AudioManager _AudioManager;

        Sequence _idleSeq;
        Tween _spawnTween;

        Vector3 _originalScale;
        float _baseY;

        private ItemType _itemType => itemConfig.itemType;
        private int _MinItemRequire => itemConfig.MinItemRequire;
        private int _MaxItemRequire => itemConfig.MaxItemRequire;
        private int _OxyBonus => itemConfig.OxyBonus;
        private Sprite _itemIcon => itemConfig.itemIcon;
        private AudioClip[] _clip => itemConfig.clip;
        private AudioClip _monsterClip => itemConfig.monsterClip;

        public ItemType itemType => _itemType;
        public Sprite itemIcon => _itemIcon;

        void Awake()
        {
            _AudioManager = bJakGZQ3_AudioManager.Instance;
            _originalScale = transform.localScale;
        }

        void OnEnable()
        {
            _baseY = transform.position.y;
            transform.localScale = _originalScale * popScaleFrom;
            StopIdleTween();
            StartSpawnPop();
        }

        void OnDisable()
        {
            StopSpawnPop();
            StopIdleTween();
        }

        public void Collect(bJakGZQ3_Player player)
        {
            if (_itemType == ItemType.GUN)
            {
                HandleGunPickup(player);
            }
            else
            {
                player.OxyGen.AddOxygen(_OxyBonus);
                bJakGZQ3_DataManager.Instance?.OnPlayerCollectItem(_itemType);
            }

            if (_clip != null && _clip.Length > 0)
            {
                int index = Random.Range(0, _clip.Length);
                _AudioManager?.PlaySfx(_clip[index]);
            }

            Destroy(gameObject);
        }

        void HandleGunPickup(bJakGZQ3_Player player)
        {
            player.EquipGun();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out bJakGZQ3_Player p))
            {
                Collect(p);
            }
            else
            {
                _AudioManager?.PlaySfx(_monsterClip);
                Destroy(this.gameObject);
            }
        }

        public int GetRandomRequire()
        {
            return Random.Range(_MinItemRequire, _MaxItemRequire + 1);
        }

        void StartSpawnPop()
        {
            StopSpawnPop();

            if (!isActiveAndEnabled) return;

            _spawnTween = transform
                .DOScale(_originalScale, popDuration)
                .SetEase(popEase)
                .OnComplete(() =>
                {
                    StartIdleTween();
                });
        }

        void StopSpawnPop()
        {
            if (_spawnTween != null && _spawnTween.IsActive())
            {
                _spawnTween.Kill();
            }
            _spawnTween = null;
        }

        void StartIdleTween()
        {
            StopIdleTween();
            if (!isActiveAndEnabled) return;

            float spinDuration = (spinSpeedDegPerSec <= 0f)
                ? 999f
                : 360f / spinSpeedDegPerSec;

            Tween spinTween = transform
                .DORotate(
                    new Vector3(0f, 360f, 0f),
                    spinDuration,
                    RotateMode.WorldAxisAdd
                )
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);

            float upY = _baseY + bobUpOffset;
            float downY = _baseY - bobDownOffset;

            Tween bobTween = transform
                .DOMoveY(upY, bobDuration)
                .SetEase(bobEase)
                .SetLoops(-1, LoopType.Yoyo)
                .From(downY);

            _idleSeq = DOTween.Sequence();
            _idleSeq.Join(spinTween);
            _idleSeq.Join(bobTween);
            _idleSeq.SetUpdate(false);
        }

        void StopIdleTween()
        {
            if (_idleSeq != null && _idleSeq.IsActive())
            {
                _idleSeq.Kill();
            }
            _idleSeq = null;
        }
    }
}
