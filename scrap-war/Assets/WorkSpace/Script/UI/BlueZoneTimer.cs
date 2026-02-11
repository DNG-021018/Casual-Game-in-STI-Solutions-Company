using TMPro;
using UnityEngine;

public class BlueZoneTimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    private float countdownTime;
    private bool counting;

    private void OnEnable()
    {
        GameManager.OnZoneCountdownStarted += StartCountdown;
    }

    private void OnDisable()
    {
        GameManager.OnZoneCountdownStarted -= StartCountdown;
    }

    public void StartCountdown(float time)
    {
        countdownTime = time;
        counting = true;
    }

    private void Update()
    {
        if (!counting) return;

        countdownTime -= Time.deltaTime;
        if (countdownTime <= 0f)
        {
            countdownTime = 0f;
            counting = false;
        }

        timerText.text = FormatTime(countdownTime);
    }

    private string FormatTime(float time)
    {
        if (time >= 60f)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            return $"{minutes:00}:{seconds:00}";
        }
        else
        {
            return $"{Mathf.CeilToInt(time)}s";
        }
    }
}
