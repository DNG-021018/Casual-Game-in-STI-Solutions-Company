using UnityEngine;
using DG.Tweening;
using System;

namespace Wja8YNiR_PrismPath
{
    public class Wja8YNiR_Plants : Wja8YNiR_Entities
    {
        [SerializeField] private GameObject _plantSmall;
        [SerializeField] private GameObject _plantLarge;

        [SerializeField] private float preEnlargeMult = 1.20f;
        [SerializeField] private float preEnlargeDuration = 0.12f;
        [SerializeField] private Ease preEnlargeEase = Ease.OutBack;
        [SerializeField] private float preBackOvershoot = 1.4f;

        [SerializeField] private float shrinkDuration = 0.22f;
        [SerializeField] private Ease shrinkEase = Ease.InExpo;

        [SerializeField] private float growDuration = 0.45f;
        [SerializeField] private Ease growEase = Ease.OutBack;
        [SerializeField] private float growBackOvershoot = 1.6f;

        private Sequence _seq;
        private Vector3 _smallInitScale = Vector3.one;
        private Vector3 _largeInitScale = Vector3.one;

        public Action TriggerPlant;

        Wja8YNiR_LevelManager levelManager;

        void Awake()
        {
            if (_plantSmall) _smallInitScale = _plantSmall.transform.localScale;
            if (_plantLarge) _largeInitScale = _plantLarge.transform.localScale;
        }

        void OnEnable()
        {
            TriggerPlant += GrowUp;
        }

        void Start()
        {
            levelManager = Wja8YNiR_LevelManager.Instance;
        }

        void OnDisable()
        {
            TriggerPlant -= GrowUp;
        }

        private void GrowUp()
        {
            if (!levelManager) return;

            if (!levelManager.FinishGame())
            {
                return;
            }

            if (_plantSmall == null || _plantLarge == null)
            {
                return;
            }

            if (_seq != null && _seq.IsActive()) _seq.Kill();

            if (!_plantSmall.activeSelf) _plantSmall.SetActive(true);
            _plantSmall.transform.localScale = _smallInitScale;

            _seq = DOTween.Sequence().SetLink(gameObject);

            _seq.Append(
                _plantSmall.transform
                    .DOScale(_smallInitScale * preEnlargeMult, preEnlargeDuration)
                    .SetEase(preEnlargeEase, preBackOvershoot)
            );

            _seq.Append(
                _plantSmall.transform
                    .DOScale(Vector3.zero, shrinkDuration)
                    .SetEase(shrinkEase)
            );

            _seq.AppendCallback(() =>
            {
                _plantSmall.SetActive(false);
                _plantLarge.SetActive(true);
                _plantLarge.transform.localScale = Vector3.zero;
            });

            _seq.Append(
                _plantLarge.transform
                    .DOScale(_largeInitScale, growDuration)
                    .SetEase(growEase, growBackOvershoot)
            );

            _seq.OnComplete(() =>
            {
                _plantLarge.transform.localScale = _largeInitScale;
                Wja8YNiR_GameManager.Instance?.SetState(GameState.Win);
            });
        }
    }
}
