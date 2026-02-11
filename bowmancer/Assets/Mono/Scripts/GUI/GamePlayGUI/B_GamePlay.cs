using System.Collections;
using TMPro;
using UnityEngine;

namespace Bowmancer
{
    public class B_GamePlay : B_UIPage
    {
        [Header("Panels")]
        [SerializeField] Panels Panel;

        [Header("Buttons")]
        [SerializeField] B_UIButton daiyRewardButton;
        [SerializeField] B_UIButton shopUpgradeButton;
        [SerializeField] B_UIButton pauseButton;

        [Header("Texts")]
        [SerializeField] TextMeshProUGUI currentLevelText;
        [SerializeField] TextMeshProUGUI coinText;

        [Header("Coin Animation")]
        [SerializeField] private float coinCountDuration = 0.5f;

        [Header("Notify Settings")]
        [SerializeField] B_JustPop coinNotifyPop;

        B_BaseUI parent;
        B_DailyRewardManager _dailyRewardManager;
        B_CurrencyManager _currencyManager;
        B_GameManager _gameManager;
        private Coroutine _coinCountCoroutine;
        private int _currentDisplayValue;

        void Awake()
        {
            _dailyRewardManager = B_DailyRewardManager.Instance;
            _currencyManager = B_CurrencyManager.Instance;
            _gameManager = B_GameManager.Instance;
        }

        public override void Init(B_BaseUI parent)
        {
            base.Init(parent);
            this.parent = parent;
        }

        void Start()
        {
            coinText.text = _currencyManager.GetCoins().ToString();
        }

        void OnEnable()
        {
            daiyRewardButton.Bind(() =>
            {
                parent.Open(UIPageId.DailyReward);
            });

            pauseButton.Bind(() =>
            {
                parent.Open(UIPageId.PauseMenu);
            });

            shopUpgradeButton.Bind(() =>
            {
                parent.Open(UIPageId.ShopUpgradeMenu);
            });

            _currencyManager.OnCoinsChanged += UpdateCoinDisplay;
        }

        void OnDisable()
        {
            daiyRewardButton.UnBind();
            shopUpgradeButton.UnBind();
            pauseButton.UnBind();
            _currencyManager.OnCoinsChanged -= UpdateCoinDisplay;
        }

        public override IEnumerator Show()
        {
            UpdateNotifyDisplay();
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            yield return base.Show();
            int level = _gameManager.CurrentLevel;
            currentLevelText.text = "LEVEL " + level.ToString();
            yield break;
        }

        public override IEnumerator Hide()
        {
            UpdateNotifyDisplay();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            yield break;
        }

        private void UpdateNotifyDisplay()
        {
            coinNotifyPop.gameObject.SetActive(_dailyRewardManager.HasRewardToday());
        }

        private void UpdateCoinDisplay(int targetValue)
        {
            if (_coinCountCoroutine != null)
            {
                StopCoroutine(_coinCountCoroutine);
            }
            _coinCountCoroutine = StartCoroutine(CountCoinAnimation(targetValue));
        }

        private IEnumerator CountCoinAnimation(int targetValue)
        {
            int startValue = _currentDisplayValue;
            float elapsedTime = 0f;

            while (elapsedTime < coinCountDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / coinCountDuration;
                int displayValue = Mathf.RoundToInt(Mathf.Lerp(startValue, targetValue, progress));
                coinText.text = displayValue.ToString();
                yield return null;
            }

            coinText.text = targetValue.ToString();
            _currentDisplayValue = targetValue;
        }
    }
}
