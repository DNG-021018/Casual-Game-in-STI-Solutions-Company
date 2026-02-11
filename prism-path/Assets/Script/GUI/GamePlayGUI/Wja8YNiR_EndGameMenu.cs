using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Wja8YNiR_PrismPath
{
    public class Wja8YNiR_EndGameMenu : Wja8YNiR_UIPage
    {
        [Header("Panel")]
        [SerializeField] Panels Panel;

        [Header("Button")]
        [SerializeField] Wja8YNiR_UIButton nextLevelBtn;
        [SerializeField] Wja8YNiR_UIButton replayBtn;
        [SerializeField] Wja8YNiR_UIButton exitBtn;

        [Header("TMP")]
        [SerializeField] TextMeshProUGUI finalTime;
        [SerializeField] Wja8YNiR_Star[] finalStar;

        [Header("Sunlight")]
        [SerializeField] RectTransform sunlight;
        [SerializeField, Range(10f, 360f)] float rotationSpeed = 90f;

        [Header("Audio")]
        [SerializeField] AudioClip AwakeClip;
        [SerializeField] AudioClip[] starClips = new AudioClip[3];

        [Tooltip("Nếu bật, delay giữa các sao = độ dài clip của sao trước đó")]
        [SerializeField] bool useClipLengthAsGap = true;

        [Tooltip("Thêm 1 chút khoảng nghỉ giữa các sao (giây, realtime)")]
        [SerializeField, Range(0f, 0.5f)] float extraGap = 0.06f;
        Sequence _starSeq;

        LevelHUDSnapshot info;
        Wja8YNiR_LevelManager levelManager;

        public override void Init(Wja8YNiR_BaseUI parent)
        {
            base.Init(parent);
        }

        void Start()
        {
            levelManager = Wja8YNiR_LevelManager.Instance;

            if (nextLevelBtn != null) nextLevelBtn.Bind(() =>
            {
                Wja8YNiR_GameManager.Instance.currentLevel++;
                Wja8YNiR_LoadingScreenRoot.Instance.LoadScene("GamePlay");
            });

            if (replayBtn != null) replayBtn.Bind(() =>
            {
                Wja8YNiR_LoadingScreenRoot.Instance.LoadScene("GamePlay");
            });

            if (exitBtn != null) exitBtn.Bind(() =>
            {
                Wja8YNiR_LoadingScreenRoot.Instance.LoadScene("StartGame");
            });

            if (nextLevelBtn != null) nextLevelBtn.gameObject.SetActive(!levelManager.checkNextLevelInvalid());
            UpdateHUDInfo();
            RevealStarsSequentially();
        }

        void StartSunlightRotation()
        {
            if (sunlight == null) return;
            sunlight.DOKill();
            float duration = 360f / rotationSpeed;

            sunlight
                .DORotate(new Vector3(0f, 0f, 360f), duration, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .SetUpdate(true);
        }

        void StopSunlightRotation()
        {
            if (sunlight != null)
            {
                sunlight.DOKill();
            }
        }

        void OnDestroy()
        {
            StopSunlightRotation();
            _starSeq?.Kill();
            if (nextLevelBtn != null) nextLevelBtn.UnBind();
            if (replayBtn != null) replayBtn.UnBind();
            if (exitBtn != null) exitBtn.UnBind();
        }

        void UpdateHUDInfo()
        {
            info = levelManager.GetHUDValue();

            if (finalStar != null && finalStar.Length >= 3)
            {
                finalStar[0]?.OnText(levelManager.FirstStarRequire);
                finalStar[1]?.OnText(levelManager.SecondStarRequire);
                finalStar[2]?.OnText(levelManager.ThirdStarRequire);
            }

            if (finalTime != null)
                finalTime.text = FormatTime(info.timeRemain);
        }

        string FormatTime(float t)
        {
            if (t < 0f) t = 0f;
            int m = Mathf.FloorToInt(t / 60f);
            int s = Mathf.FloorToInt(t % 60f);
            return $"{m:00}:{s:00}";
        }

        protected override void CacheStartPositions()
        {
            if (Panel.panel) Panel.panel.anchoredPosition = new Vector2(0, -offscreenPadding);
        }

        public override IEnumerator Show(object ctx = null)
        {
            base.Show(ctx);
            CacheStartPositions();
            StartSunlightRotation();

            if (Wja8YNiR_AudioManager.Instance && AwakeClip)
            {
                Wja8YNiR_AudioManager.Instance.PlaySfx(AwakeClip);
            }

            yield return ShowScalePanels(
                duration, showEase, 0f, 1f,
                (Panel.panel, Vector3.zero, Panel.panel.localScale)
            );
        }

        public override IEnumerator Hide()
        {
            base.Hide();

            StopSunlightRotation();
            _starSeq?.Kill();
            CacheStartPositions();

            yield return HideScalePanels(
                duration, hideEase, 1f, 0f,
                (Panel.panel, Panel.panel.localScale, Vector3.zero)
            );
        }

        void RevealStarsSequentially()
        {
            _starSeq?.Kill();
            if (finalStar == null || finalStar.Length == 0) return;

            int count = Mathf.Clamp(info.StarReceive, 0, finalStar.Length);
            _starSeq = DOTween.Sequence().SetUpdate(true);

            for (int i = 0; i < count; i++)
            {
                int idx = i;

                _starSeq.AppendCallback(() =>
                {
                    var slot = finalStar[idx];
                    if (slot == null) return;

                    AudioClip landClip = (starClips != null && idx < starClips.Length) ? starClips[idx] : null;
                    FlyStar(slot.TargetAnchor, landClip, onArrive: slot.OnStarPop);
                });

                if (idx < count - 1)
                {
                    float gap = extraGap + 0.12f;
                    if (useClipLengthAsGap && starClips != null && idx < starClips.Length && starClips[idx])
                    {
                        gap = starClips[idx].length + extraGap;
                    }
                    _starSeq.AppendInterval(gap);
                }
            }
        }

        void FlyStar(RectTransform target, AudioClip landClip, Action onArrive)
        {
            if (!target) return;

            var root = target.parent as RectTransform;
            if (!root) return;

            Sprite sp = null;
            Vector2 finalSize = target.sizeDelta;
            Vector2 pivot = target.pivot;

            var starSlot = target.GetComponent<Wja8YNiR_Star>();
            if (starSlot != null && starSlot.TryGetWinStarVisual(out var winSprite, out var winRect))
            {
                sp = winSprite;
                finalSize = winRect.sizeDelta;
                pivot = winRect.pivot;
            }
            else
            {
                var targetImg = target.GetComponent<Image>();
                if (targetImg) sp = targetImg.sprite;
            }

            var go = new GameObject("FlyStar", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(root, false);

            go.AddComponent<UnityEngine.UI.LayoutElement>().ignoreLayout = true;

            rt.pivot = pivot;
            rt.anchorMin = rt.anchorMax = Vector2.one * 0.5f;
            rt.sizeDelta = finalSize;
            rt.localScale = Vector3.one * 0.6f;

            var img = go.GetComponent<Image>();
            img.sprite = sp;
            img.raycastTarget = false;
            img.preserveAspect = true;

            var cg = go.GetComponent<CanvasGroup>();
            cg.alpha = 0f;

            var canvas = root.GetComponentInParent<Canvas>();
            var cam = (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;

            Vector3 worldCenter = target.TransformPoint(target.rect.center);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                root,
                RectTransformUtility.WorldToScreenPoint(cam, worldCenter),
                cam,
                out Vector2 targetLocalCenter
            );

            Rect r = root.rect;
            rt.anchoredPosition = targetLocalCenter + new Vector2(r.width * 0.65f, UnityEngine.Random.Range(-r.height * 0.12f, r.height * 0.12f));

            const float travelTime = 0.6f;
            var seq = DOTween.Sequence().SetUpdate(true);
            seq.Append(cg.DOFade(1f, 0.1f));
            seq.Join(rt.DOAnchorPos(targetLocalCenter, travelTime).SetEase(Ease.OutCubic));
            seq.Join(rt.DOLocalRotate(new Vector3(0, 0, -720f), travelTime, RotateMode.FastBeyond360).SetEase(Ease.Linear));
            seq.Join(rt.DOScale(1.1f, travelTime * 0.6f).SetEase(Ease.OutBack, 1.6f));

            seq.OnComplete(() =>
            {
                if (Wja8YNiR_AudioManager.Instance && landClip)
                    Wja8YNiR_AudioManager.Instance.PlaySfx(landClip);

                onArrive?.Invoke();
                Destroy(go);
            });
        }
    }
}
