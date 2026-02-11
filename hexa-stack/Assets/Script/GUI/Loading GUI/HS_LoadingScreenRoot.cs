using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HexaStack
{
    public class HS_LoadingScreenRoot : MonoBehaviour
    {
        public static HS_LoadingScreenRoot Instance { get; private set; }

        [Header("Refs")]
        [SerializeField] private CanvasGroup overlay;
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI progressText;

        [Header("Timing")]
        [SerializeField] private float fadeDuration = 0.25f;
        [SerializeField] private float minShowTime = 0.6f;

        [Header("Tweak")]
        [SerializeField] private bool blockRaycasts = true;
        [SerializeField] private bool ignoreTimeScale = true;

        public bool IsBusy { get; private set; }
        Coroutine co;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            if (!overlay) overlay = GetComponentInChildren<CanvasGroup>(true);
            // HideInstant();
        }

        void Start()
        {
            LoadScene("GamePlay", LoadSceneMode.Single);
        }

        public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, Action onAfter = null)
        {
            if (IsBusy) { Debug.LogWarning("[SceneTransition] Already loading."); return; }
            co = StartCoroutine(CoLoad(sceneName, mode, onAfter));
        }

        public void LoadScene(int buildIndex, LoadSceneMode mode = LoadSceneMode.Single, Action onAfter = null)
        {
            if (IsBusy) { Debug.LogWarning("[SceneTransition] Already loading."); return; }
            co = StartCoroutine(CoLoad(buildIndex, mode, onAfter));
        }

        IEnumerator CoLoad(string sceneName, LoadSceneMode mode, Action onAfter)
        {
            IsBusy = true;

            yield return Fade(1f);
            var startT = Time.unscaledTime;

            var op = SceneManager.LoadSceneAsync(sceneName, mode);
            op.allowSceneActivation = false;

            yield return TrackProgress(op);

            while (Time.unscaledTime - startT < minShowTime) yield return null;

            UpdateProgressUI(1f);
            op.allowSceneActivation = true;

            while (!op.isDone) yield return null;

            yield return null;

            onAfter?.Invoke();
            yield return Fade(0f);
            IsBusy = false;
        }

        IEnumerator CoLoad(int buildIndex, LoadSceneMode mode, Action onAfter)
        {
            IsBusy = true;
            yield return Fade(1f);
            var startT = Time.unscaledTime;

            var op = SceneManager.LoadSceneAsync(buildIndex, mode);
            op.allowSceneActivation = false;

            yield return TrackProgress(op);

            while (Time.unscaledTime - startT < minShowTime) yield return null;

            UpdateProgressUI(1f);
            op.allowSceneActivation = true;
            while (!op.isDone) yield return null;
            yield return null;

            onAfter?.Invoke();
            yield return Fade(0f);
            IsBusy = false;
        }

        IEnumerator TrackProgress(AsyncOperation op)
        {
            float shown = 0f;
            while (op.progress < 0.9f)
            {
                float target = Mathf.Clamp01(op.progress / 0.9f);
                shown = Mathf.MoveTowards(shown, target, (ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime) * 1.5f);
                UpdateProgressUI(shown);
                yield return null;
            }

            while (shown < 0.98f)
            {
                shown = Mathf.MoveTowards(shown, 0.98f, (ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime) * 1.5f);
                UpdateProgressUI(shown);
                yield return null;
            }
        }

        void UpdateProgressUI(float t01)
        {
            if (progressBar) progressBar.value = t01;
            if (progressText) progressText.text = Mathf.RoundToInt(t01 * 100f) + "%";
        }

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
    }
}
