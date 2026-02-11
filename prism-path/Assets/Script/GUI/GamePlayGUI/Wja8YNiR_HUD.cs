using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Wja8YNiR_PrismPath
{
    public class Wja8YNiR_HUD : MonoBehaviour
    {
        [Header("Timer UI")]
        [SerializeField] TMP_Text timerTMP;

        [Header("Mirror UI")]
        [SerializeField] TMP_Text mirrorTMP;

        [Header("Level Name")]
        [SerializeField] TMP_Text levelNameTMP;

        private Wja8YNiR_LevelManager _levelManager;

        void Start()
        {
            _levelManager = Wja8YNiR_LevelManager.Instance;
            if (_levelManager == null)
            {
                Debug.LogWarning("[HUD] No LevelManager found");
                return;
            }
            else
            {
                _levelManager.OnLevelInitialized += OnValueChange;
                _levelManager.OnHUDChanged += OnValueChange;
                _levelManager.OnTimeExpired += OnTimeExpired;
                OnValueChange(_levelManager.GetHUDValue());
            }
        }

        void OnDisable()
        {
            if (!_levelManager) return;
            _levelManager.OnLevelInitialized -= OnValueChange;
            _levelManager.OnHUDChanged -= OnValueChange;
            _levelManager.OnTimeExpired -= OnTimeExpired;
        }

        void OnValueChange(LevelHUDSnapshot s)
        {
            string tStr = FormatTime(s.timeRemain);
            if (timerTMP) timerTMP.text = tStr;
            if (levelNameTMP) levelNameTMP.text = s.levelName;
            if (mirrorTMP) mirrorTMP.text = $"{s.mirrorRemain}/{s.mirrorLimit}";
        }

        void OnTimeExpired()
        {
            OnValueChange(_levelManager.GetHUDValue());
        }

        string FormatTime(float t)
        {
            if (t < 0f) t = 0f;
            int m = Mathf.FloorToInt(t / 60f);
            int s = Mathf.FloorToInt(t % 60f);
            return $"{m:00}:{s:00}";
        }
    }
}
