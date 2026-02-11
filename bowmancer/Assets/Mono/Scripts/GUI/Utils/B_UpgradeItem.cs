using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bowmancer
{
    public class B_UpgradeItem : MonoBehaviour
    {
        [SerializeField] private AudioClip claimRewardClip;
        [SerializeField] private TextMeshProUGUI upgradeNameText;
        [SerializeField] private TextMeshProUGUI upgradeLevelText;
        [SerializeField] private Image upgradeIconImage;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private B_UIButton selectButton;

        private B_BaseUpgrade currentUpgrade;
        private B_AudioManager _audioManager;
        B_BaseUI parent;

        private void Awake()
        {
            _audioManager = B_AudioManager.Instance;
            if (selectButton == null)
            {
                selectButton = GetComponent<B_UIButton>();
            }
        }

        private void OnEnable()
        {
            if (selectButton != null)
            {
                selectButton.Bind(OnUpgradeSelected);
            }
        }

        private void OnDisable()
        {
            if (selectButton != null)
            {
                selectButton.UnBind();
            }
        }

        public void SetUpgradeInfo(B_BaseUpgrade upgrade, B_BaseUI parent)
        {
            currentUpgrade = upgrade;
            this.parent = parent;

            if (upgrade == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            upgradeNameText.text = upgrade.UpgradeName.ToString();
            upgradeIconImage.sprite = upgrade.UpgradeIcon;
            descriptionText.text = upgrade.Description;

            int currentLevel = B_UpgradeManager.Instance.GetUpgradeLevel(upgrade);
            upgradeLevelText.text = $"Level {currentLevel}";
        }

        private void OnUpgradeSelected()
        {
            if (currentUpgrade == null) return;

            bool success = B_UpgradeManager.Instance.ApplyUpgrade(currentUpgrade);
            _audioManager.PlaySfx(claimRewardClip);
            if (success)
            {
                parent.CloseAll();
                parent.Open(UIPageId.GamePlay);
                B_GameManager.Instance.SetState(GameState.Play);
            }
        }
    }
}
