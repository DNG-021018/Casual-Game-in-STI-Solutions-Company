using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bowmancer
{
    public class B_Healthbar : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image HealthbarSprite;
        [SerializeField] private Image DelayedHealthbarSprite;
        [SerializeField] private TextMeshProUGUI HealthbarText;

        [Header("Tuning")]
        [SerializeField] private float ReduceSpeed = 2f;
        [SerializeField] private float DelayDuration = 0.3f;

        private float _target = 1f;
        private float _delayedTarget = 1f;
        private float _maxHealth = 1f;

        private float _delayTimer = 0f;
        private Camera _cam;

        void Start()
        {
            _cam = Camera.main;
        }

        public void Init(float maxHealth)
        {
            _maxHealth = maxHealth;
            _target = 1f;
            _delayedTarget = 1f;

            HealthbarSprite.fillAmount = 1f;
            DelayedHealthbarSprite.fillAmount = 1f;

            HealthbarText.text = maxHealth.ToString();
        }

        public void SetHealth(float health)
        {
            _target = health / _maxHealth;
            HealthbarSprite.fillAmount = _target;

            HealthbarText.text = Mathf.CeilToInt(health).ToString();

            _delayTimer = DelayDuration;
        }

        void Update()
        {
            FaceCamera();
            UpdateDelayedBar();
        }

        void UpdateDelayedBar()
        {
            if (_delayTimer > 0f)
            {
                _delayTimer -= Time.deltaTime;
                return;
            }

            _delayedTarget = _target;

            float fillAmount = Mathf.MoveTowards(DelayedHealthbarSprite.fillAmount, _delayedTarget, Time.deltaTime * ReduceSpeed);
            DelayedHealthbarSprite.fillAmount = fillAmount;
        }

        void FaceCamera()
        {
            Vector3 directionToCamera = transform.position - _cam.transform.position;
            Vector3 eulerAngles = Quaternion.LookRotation(directionToCamera).eulerAngles;
            transform.rotation = Quaternion.Euler(eulerAngles.x, 0, 0);
        }
    }
}
