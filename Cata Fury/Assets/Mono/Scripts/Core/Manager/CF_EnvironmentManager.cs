using DG.Tweening;
using UnityEngine;

namespace CataFury
{
    public class CF_EnvironmentManager : MonoBehaviour
    {
        [Header("Threshold")]
        [SerializeField] private int scorePerCycle = 24;

        [Header("Directional Light")]
        [SerializeField] private Light directionalLight;

        [Header("Day Settings")]
        [SerializeField] private Color dayLightColor = Color.white;
        [SerializeField] private float dayLightIntensity = 1f;
        [SerializeField] private Vector3 dayLightRotation = new Vector3(50f, -30f, 0f);

        [Header("Night Settings")]
        [SerializeField] private Color nightLightColor = new Color(0.1f, 0.15f, 0.4f);
        [SerializeField] private float nightLightIntensity = 0.3f;
        [SerializeField] private Vector3 nightLightRotation = new Vector3(30f, 150f, 0f);

        [Header("Transition")]
        [SerializeField] private float transitionDuration = 1.5f;

        private CF_ScoreManager _scoreManager;
        private bool _isDay = true;
        private int _lastCycle = 0;
        private Sequence _transitionSeq;

        void Start()
        {
            _scoreManager = ServiceLocator.Get<CF_ScoreManager>();
            if (_scoreManager != null)
                _scoreManager.OnScoreChanged += OnScoreChanged;

            ApplyEnvironmentInstant(_isDay);
        }

        void OnDestroy()
        {
            if (_scoreManager != null)
                _scoreManager.OnScoreChanged -= OnScoreChanged;

            _transitionSeq?.Kill();
        }

        private void OnScoreChanged(int score)
        {
            int currentCycle = score / scorePerCycle;
            if (currentCycle == _lastCycle) return;

            _lastCycle = currentCycle;
            _isDay = !_isDay;
            TransitionTo(_isDay);
        }

        private void TransitionTo(bool toDay)
        {
            if (directionalLight == null) return;

            _transitionSeq?.Kill();
            _transitionSeq = DOTween.Sequence();

            Color targetColor = toDay ? dayLightColor : nightLightColor;
            float targetIntensity = toDay ? dayLightIntensity : nightLightIntensity;
            Vector3 targetRotation = toDay ? dayLightRotation : nightLightRotation;

            _transitionSeq.Join(
                DOTween.To(
                    () => directionalLight.color,
                    x => directionalLight.color = x,
                    targetColor,
                    transitionDuration
                )
            );

            _transitionSeq.Join(
                DOTween.To(
                    () => directionalLight.intensity,
                    x => directionalLight.intensity = x,
                    targetIntensity,
                    transitionDuration
                )
            );

            _transitionSeq.Join(
                directionalLight.transform
                    .DORotate(targetRotation, transitionDuration)
                    .SetEase(Ease.InOutSine)
            );
        }

        private void ApplyEnvironmentInstant(bool isDay)
        {
            if (directionalLight == null) return;

            directionalLight.color = isDay ? dayLightColor : nightLightColor;
            directionalLight.intensity = isDay ? dayLightIntensity : nightLightIntensity;
            directionalLight.transform.rotation = Quaternion.Euler(
                isDay ? dayLightRotation : nightLightRotation
            );
        }

        public void ResetEnvironment()
        {
            _lastCycle = 0;
            _isDay = true;
            _transitionSeq?.Kill();
            ApplyEnvironmentInstant(true);
        }
    }
}
