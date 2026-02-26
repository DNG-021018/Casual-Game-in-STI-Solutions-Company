using UnityEngine;

namespace DoublesideZ
{
    [DefaultExecutionOrder(-100)]
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Global Services to Register")]
        [SerializeField] private DZ_CurrencyManager currencyManager;
        [SerializeField] private DZ_DailyRewardManager dailyRewardManager;
        [SerializeField] private DZ_WeaponManager weaponManager;
        [SerializeField] private DZ_AudioManager audioManager;
        [SerializeField] private DZ_UIManager uiManager;

        [Header("Loading Screen")]
        [SerializeField] DZ_LoadingScreenManager loadingScreenRoot;

        void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            Physics.reuseCollisionCallbacks = true;

            loadingScreenRoot.EnableExternalVisualControl(true);
            loadingScreenRoot.ShowVisual();

            RegisterServices();

            loadingScreenRoot.EndManualLoading();
        }

        void RegisterServices()
        {
            loadingScreenRoot.BeginManualLoading();
            loadingScreenRoot.SetManualProgress(0.1f);

            if (currencyManager != null)
            {
                currencyManager.Init();
                ServiceLocator.Register(currencyManager);
            }

            if (dailyRewardManager != null)
            {
                dailyRewardManager.Init();
                ServiceLocator.Register(dailyRewardManager);
            }

            if (weaponManager != null)
            {
                weaponManager.Init(currencyManager);
                ServiceLocator.Register(weaponManager);
            }

            if (audioManager != null)
            {
                audioManager.Init();
                ServiceLocator.Register(audioManager);
            }

            if (uiManager != null)
            {
                ServiceLocator.Register(uiManager);
            }

            loadingScreenRoot.SetManualProgress(0.5f);
        }
    }
}
