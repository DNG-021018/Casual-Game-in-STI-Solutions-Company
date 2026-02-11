using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Wja8YNiR_PrismPath
{
    public class Wja8YNiR_LoadingScreenRoot : MonoBehaviour
    {
        public static Wja8YNiR_LoadingScreenRoot Instance { get; private set; }

        [Header("Refs")]
        [SerializeField] private CanvasGroup overlay;       // panel đen + UI loading
        [SerializeField] private Slider progressBar;        // optional
        [SerializeField] private TextMeshProUGUI progressText;         // optional (nếu xài TMP thì đổi sang TextMeshProUGUI)

        [Header("Timing")]
        [SerializeField] private float fadeDuration = 0.25f;
        [SerializeField] private float minShowTime = 0.6f;  // đảm bảo không chớp

        [Header("Tweak")]
        [SerializeField] private bool blockRaycasts = true; // chặn click xuyên trong lúc load
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
            HideInstant();
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
            // optional: GameManager.I?.SetState(GameState.ChangeScene);

            yield return Fade(1f);
            var startT = Time.unscaledTime;

            var op = SceneManager.LoadSceneAsync(sceneName, mode);
            op.allowSceneActivation = false;

            yield return TrackProgress(op);

            // đảm bảo đủ min show time
            while (Time.unscaledTime - startT < minShowTime) yield return null;

            // finish 1.0 rồi mới active
            UpdateProgressUI(1f);
            op.allowSceneActivation = true;

            // đợi xong hẳn
            while (!op.isDone) yield return null;

            // 1 frame cho scene mới ổn định
            yield return null;

            onAfter?.Invoke();
            yield return Fade(0f);
            IsBusy = false;
            // optional: GameManager.I?.SetState(GameState.Playing);
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
            // Unity trả về progress 0..0.9 trước khi allowSceneActivation
            float shown = 0f;
            while (op.progress < 0.9f)
            {
                float target = Mathf.Clamp01(op.progress / 0.9f); // map 0..0.9 -> 0..1
                shown = Mathf.MoveTowards(shown, target, (ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime) * 1.5f);
                UpdateProgressUI(shown);
                yield return null;
            }

            // chốt tới 0.95-1.0 để cảm giác mượt
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
