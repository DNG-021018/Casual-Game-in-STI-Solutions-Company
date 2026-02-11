using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class Loading : MonoBehaviour
{
    [Header("Loading Components")]
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private CanvasGroup loadingCanvasGroup;
    
    [Header("Settings")]
    [SerializeField] private float loadingDuration = 5f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private bool useRandomSpeed = true;
    [SerializeField] private float minLoadingDuration = 1f; // Minimum loading time for UI transitions

    public static System.Action OnLoadingComplete;
    private bool isInitialLoad = true;

    private void Start()
    {
        StartCoroutine(LoadingSequence());
    }

    void OnEnable()
    {
        // Only start loading sequence if this is not the initial activation
        if (!isInitialLoad)
        {
            StartCoroutine(LoadingSequence());
        }
    }

    private IEnumerator LoadingSequence()
    {
        SetProgress(0f);
        SetCanvasState(1f, true, true);

        // Use shorter duration for UI transitions after initial load
        float currentLoadingDuration = isInitialLoad ? loadingDuration : minLoadingDuration;
        
        yield return StartCoroutine(SimulateLoading(currentLoadingDuration));

        UpdateLoadingText("Loading Complete!");
        
        // Notify completion before fading out
        OnLoadingComplete?.Invoke();
        
        // Wait a frame to ensure target UI is shown
        yield return null;
        
        yield return StartCoroutine(FadeOut());

        gameObject.SetActive(false);
        
        // Mark that initial load is complete
        if (isInitialLoad)
            isInitialLoad = false;
    }
    
    private IEnumerator SimulateLoading(float duration)
    {
        float progress = 0f;
        float elapsedTime = 0f;
        
        while (progress < 1f)
        {
            elapsedTime += Time.deltaTime;
            
            float speedMultiplier = useRandomSpeed ? Random.Range(0.5f, 2f) : 1f;
            progress = useRandomSpeed 
                ? Mathf.Clamp01(progress + (Time.deltaTime / duration) * speedMultiplier)
                : elapsedTime / duration;
            
            SetProgress(progress);
            yield return null;
        }
    }
    
    private IEnumerator FadeOut()
    {
        loadingCanvasGroup.interactable = false;
        
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            loadingCanvasGroup.alpha = alpha;
            yield return null;
        }
        
        SetCanvasState(0f, false, false);
    }
    
    private void SetProgress(float progress)
    {
        if (loadingSlider != null)
            loadingSlider.value = progress;
        
        UpdateLoadingText($"Loading... {Mathf.RoundToInt(progress * 100)}%");
    }
    
    private void UpdateLoadingText(string text)
    {
        if (loadingText != null)
            loadingText.text = text;
    }
    
    private void SetCanvasState(float alpha, bool interactable, bool blocksRaycasts)
    {
        if (loadingCanvasGroup == null) return;
        
        loadingCanvasGroup.alpha = alpha;
        loadingCanvasGroup.interactable = interactable;
        loadingCanvasGroup.blocksRaycasts = blocksRaycasts;
    }
    
    [ContextMenu("Restart Loading")]
    public void RestartLoading()
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        StartCoroutine(LoadingSequence());
    }
}