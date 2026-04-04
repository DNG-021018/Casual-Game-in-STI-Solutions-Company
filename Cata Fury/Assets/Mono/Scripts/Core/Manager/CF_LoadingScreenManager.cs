using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CataFury
{
    public class CF_LoadingScreenManager : Singleton<CF_LoadingScreenManager>
    {
        [Header("Scene to load on start")]
        [SerializeField] private bool loadOnStart = true;
        [SerializeField] private string scenename = "MainMenu";

        [Header("Refs")]
        [SerializeField] private CanvasGroup overlay;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI progressText;

        [Header("Timing")]
        [SerializeField] private float fadeDuration = 0.25f;
        [SerializeField] private float minShowTime = 0.6f;

        [Header("Tweak")]
        [SerializeField] private bool hideInstance = false;
        [SerializeField] private bool blockRaycasts = true;
        [SerializeField] private bool ignoreTimeScale = true;

        [Header("Weights")]
        [SerializeField] private float sceneWeight = 0.7f;
        [SerializeField] private float manualWeight = 0.3f;

        public bool IsBusy { get; private set; }

        float manualProgress = 0f;
        bool manualActive = false;

        bool externalVisualControl = false;

        protected override void Awake()
        {
            base.Awake();
        }

        void Start()
        {
            if (!overlay) overlay = GetComponentInChildren<CanvasGroup>(true);

            if (hideInstance) HideInstant();

            if (loadOnStart)
            {
                LoadSceneWithName(scenename, LoadSceneMode.Additive);
            }
        }

        #region  EXTERNAL VISUAL CONTROL

        public void EnableExternalVisualControl(bool enable)
        {
            externalVisualControl = enable;
        }

        public void ShowVisual()
        {
            StartCoroutine(Fade(1f));
        }

        public void HideVisual()
        {
            StartCoroutine(Fade(0f));
        }

        public IEnumerator ShowVisualAndWait()
        {
            yield return Fade(1f);
        }

        public IEnumerator HideVisualAndWait()
        {
            yield return Fade(0f);
        }

        #endregion

        #region  MANUAL LOADING

        public void BeginManualLoading()
        {
            manualActive = true;
            manualProgress = 0f;
        }

        public void SetManualProgress(float t01)
        {
            manualProgress = Mathf.Clamp01(t01);
        }

        public void EndManualLoading()
        {
            manualProgress = 1f;
            manualActive = false;
        }

        #endregion

        #region  SCENE LOAD

        public void LoadSceneWithName(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, Action onAfter = null)
        {
            if (IsBusy) return;
            StartCoroutine(CoLoad(sceneName, mode, onAfter));
        }

        IEnumerator CoLoad(string sceneName, LoadSceneMode mode, Action onAfter)
        {
            IsBusy = true;

            if (!externalVisualControl)
                yield return Fade(1f);

            var startT = Time.unscaledTime;

            var op = SceneManager.LoadSceneAsync(sceneName, mode);
            op.allowSceneActivation = false;

            yield return TrackProgress(op);

            while (Time.unscaledTime - startT < minShowTime)
                yield return null;

            UpdateProgressUI(1f);
            op.allowSceneActivation = true;

            while (!op.isDone) yield return null;
            yield return null;

            onAfter?.Invoke();

            if (!externalVisualControl)
                yield return Fade(0f);

            IsBusy = false;
        }

        #endregion

        #region  PROGRESS
        IEnumerator TrackProgress(AsyncOperation op)
        {
            float shown = 0f;

            while (op.progress < 0.9f || manualActive)
            {
                float sceneTarget = Mathf.Clamp01(op.progress / 0.9f);

                float combined = manualActive
                    ? (sceneTarget * sceneWeight + manualProgress * manualWeight)
                    : sceneTarget;

                shown = Mathf.MoveTowards(
                    shown,
                    combined,
                    (ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime) * 1.5f);

                UpdateProgressUI(shown);
                yield return null;
            }

            while (shown < 0.98f)
            {
                shown = Mathf.MoveTowards(
                    shown,
                    0.98f,
                    (ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime) * 1.5f);

                UpdateProgressUI(shown);
                yield return null;
            }
        }

        void UpdateProgressUI(float t01)
        {
            if (progressBar) progressBar.value = t01;
            if (progressText) progressText.text = "Loading... " + Mathf.RoundToInt(t01 * 100f) + "%";
        }

        #endregion

        #region  FADE
        IEnumerator Fade(float target)
        {
            if (!overlay) yield break;

            overlay.gameObject.SetActive(true);
            overlay.blocksRaycasts = blockRaycasts;
            overlay.interactable = blockRaycasts;

            float start = overlay.alpha;
            float t = 0f;

            while (t < fadeDuration)
            {
                t += (ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime);
                float k = Mathf.Clamp01(t / fadeDuration);
                overlay.alpha = Mathf.Lerp(start, target, k);
                yield return null;
            }

            overlay.alpha = target;

            if (target <= 0f)
            {
                overlay.blocksRaycasts = false;
                overlay.interactable = false;
                overlay.gameObject.SetActive(false);
            }
        }


        void HideInstant()
        {
            if (!overlay) return;
            overlay.alpha = 0f;
            overlay.blocksRaycasts = false;
            overlay.interactable = false;
            overlay.gameObject.SetActive(false);
        }
        #endregion
    }
}