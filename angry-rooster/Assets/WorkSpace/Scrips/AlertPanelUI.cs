using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AlertPanelUI : MonoBehaviour
{
    [SerializeField] private float alertDuration = 3f;
    [SerializeField] private float blinkSpeed = 2f;
    [SerializeField] private float minAlpha = 0.3f;
    [SerializeField] private float maxAlpha = 1f;

    private Image panelImage;
    private Coroutine alertCoroutine;

    private void Awake()
    {
        panelImage = GetComponent<Image>();
        if (panelImage == null)
        {
            Debug.LogError("AlertPanelUI requires an Image component!");
        }
    }

    private void OnEnable()
    {
        if (alertCoroutine != null)
        {
            StopCoroutine(alertCoroutine);
        }

        alertCoroutine = StartCoroutine(AlertRoutine());
    }

    private IEnumerator AlertRoutine()
    {
        float timer = 0f;
        while (timer < alertDuration)
        {
            timer += Time.deltaTime;

            // Tính alpha dao động kiểu ping-pong
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(Time.time * blinkSpeed, 1f));

            // Cập nhật alpha cho panel
            if (panelImage != null)
            {
                Color color = panelImage.color;
                color.a = alpha;
                panelImage.color = color;
            }

            yield return null;
        }

        gameObject.SetActive(false); // Tắt panel sau 3 giây
    }
}
