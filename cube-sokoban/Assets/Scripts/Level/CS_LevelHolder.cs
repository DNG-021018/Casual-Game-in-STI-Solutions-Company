using DG.Tweening;
using UnityEngine;

namespace CubeSokoban
{
    public class CS_LevelHolder : MonoBehaviour
    {
        [Header("Ground Groups")]
        [SerializeField] private Transform[] _GroundGroup;
        [SerializeField] private float _groundPosMultiplier = 10f;
        [SerializeField] private float _groundDuration = 0.5f;
        [SerializeField] private float _groundDelay = 0.2f;
        [SerializeField] private Ease _groundEase = Ease.OutBounce;

        [Header("Entities Groups")]
        [SerializeField] private Transform[] _EntitiesGroup;
        [SerializeField] private float _entitiesPosMultiplier = 10f;
        [SerializeField] private float _entitiesDuration = 0.5f;
        [SerializeField] private float _entitiesDelay = 0.2f;
        [SerializeField] private Ease _entitiesEase = Ease.Linear;

        private Vector3[] _groundOriginPos;
        private Vector3[] _entitiesOriginPos;

        void Awake()
        {
            if (_GroundGroup.Length <= 0)
            {
                Debug.LogError("Ground Group is empty. Please assign ground objects.");
            }

            if (_EntitiesGroup.Length <= 0)
            {
                Debug.LogError("Entities Group is empty. Please assign entity objects.");
            }

            _groundOriginPos = new Vector3[_GroundGroup.Length];

            for (int i = 0; i < _GroundGroup.Length; i++)
            {
                _groundOriginPos[i] = _GroundGroup[i].position;
                _GroundGroup[i].position += Vector3.up * _groundPosMultiplier;
            }

            _entitiesOriginPos = new Vector3[_EntitiesGroup.Length];
            for (int i = 0; i < _EntitiesGroup.Length; i++)
            {
                _entitiesOriginPos[i] = _EntitiesGroup[i].position;
                _EntitiesGroup[i].position += Vector3.up * _entitiesPosMultiplier;
            }
        }

        void Start()
        {
            CS_GameManager.Instance.SetState(GameState.InitializeLevel);
            SequenceSpawn();
        }

        private void PlayTweenSpawnGround()
        {
            if (_GroundGroup.Length == 0)
            {
                PlayTweenSpawnEntities();
                return;
            }

            int completedCount = 0;
            for (int i = 0; i < _GroundGroup.Length; i++)
            {
                _GroundGroup[i].DOMove(_groundOriginPos[i], _groundDuration)
                    .SetDelay(i * _groundDelay)
                    .SetEase(_groundEase)
                    .SetAutoKill(true)
                    .OnComplete(() =>
                    {
                        completedCount++;
                        if (completedCount == _GroundGroup.Length)
                        {
                            PlayTweenSpawnEntities();
                        }
                    });
            }
        }

        private void PlayTweenSpawnEntities()
        {
            if (_EntitiesGroup.Length == 0)
            {
                CS_LevelManager.Instance.GameStart();
                return;
            }

            int completedCount = 0;
            for (int i = 0; i < _EntitiesGroup.Length; i++)
            {
                _EntitiesGroup[i].DOMove(_entitiesOriginPos[i], _entitiesDuration)
                    .SetDelay(i * _entitiesDelay)
                    .SetEase(_entitiesEase)
                    .SetAutoKill(true)
                    .OnComplete(() =>
                    {
                        completedCount++;
                        if (completedCount == _EntitiesGroup.Length)
                        {
                            CS_LevelManager.Instance.GameStart();
                        }
                    });
            }
        }

        private void SequenceSpawn()
        {
            PlayTweenSpawnGround();
        }
    }
}
