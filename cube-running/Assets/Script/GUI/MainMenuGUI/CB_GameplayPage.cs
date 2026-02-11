using System.Collections;
using TMPro;
using UnityEngine;


namespace CB_CubeRunner
{
    public class CB_GameplayPage : CB_UIPage
    {
        [SerializeField] TextMeshProUGUI pointText;
        [SerializeField] CB_UIButton pausedButton;
        [SerializeField] TextMeshProUGUI coinText;

        CB_BaseUI _parent;
        CB_GameManager _GameManager;

        public override void Init(CB_BaseUI parent)
        {
            base.Init(parent);
            _parent = parent;

            _GameManager = CB_GameManager.Instance;
            pausedButton.Bind(() =>
            {
                _parent.Open(UIPageId.Pause, null, false);
                _GameManager.SetState(GameState.Paused);
            });

            if (_GameManager != null)
            {
                _GameManager.OnPlayerPoint += HandlePlayerPointChanged;
                _GameManager.OnCoinChanged += HandleCoinChanged;
                HandlePlayerPointChanged(_GameManager.CurrentPoint);
                HandleCoinChanged(_GameManager.TotalCoin);
            }

            UpdatePointText(0);
        }

        private void OnDestroy()
        {
            pausedButton.UnBind();

            if (_GameManager != null)
            {
                _GameManager.OnPlayerPoint -= HandlePlayerPointChanged;
                _GameManager.OnCoinChanged -= HandleCoinChanged;
            }
        }

        public override IEnumerator Show(object ctx = null)
        {
            yield return base.Show(ctx);
            canvasGroup.alpha = 1f;

            if (_GameManager != null)
            {
                HandlePlayerPointChanged(_GameManager.CurrentPoint);
            }
        }

        public override IEnumerator Hide()
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0f;
            yield break;
        }

        void HandlePlayerPointChanged(int totalPoint)
        {
            UpdatePointText(totalPoint);
        }

        void UpdatePointText(int p)
        {
            if (pointText != null)
            {
                pointText.text = p.ToString();
            }
        }

        void HandleCoinChanged(int totalCoin)
        {
            if (coinText != null)
                coinText.text = totalCoin + "/" + CB_GameManager.MAX_COIN.ToString();
        }
    }
}