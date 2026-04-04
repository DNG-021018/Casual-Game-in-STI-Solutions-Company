using UnityEngine;

namespace CataFury
{
    [DefaultExecutionOrder(-10)]
    public class SceneBootstrap : MonoBehaviour
    {
        [Header("Scene Services to Register")]
        [SerializeField] CF_PopupManager popupManager;
        [SerializeField] CF_CameraManager cameraManager;
        [SerializeField] CF_PlayerController playerController;
        [SerializeField] CF_SpawnManager spawnManager;
        [SerializeField] CF_PoolManager poolManager;
        [SerializeField] CF_ScoreManager scoreManager;
        [SerializeField] CF_EnvironmentManager environmentManager;

        void Awake()
        {
            RegisterServices();
        }

        void Start()
        {
            CF_LoadingScreenManager.Instance.SetManualProgress(1f);
            CF_LoadingScreenManager.Instance.HideVisual();
        }

        void RegisterServices()
        {
            if (popupManager != null)
            {
                ServiceLocator.Register(popupManager);
            }

            if (playerController != null)
            {
                ServiceLocator.Register(playerController);
            }

            if (cameraManager != null)
            {
                ServiceLocator.Register(cameraManager);
            }

            if (spawnManager != null)
            {
                ServiceLocator.Register(spawnManager);
            }

            if (poolManager != null)
            {
                ServiceLocator.Register(poolManager);
            }

            if (scoreManager != null)
            {
                ServiceLocator.Register(scoreManager);
            }

            if (environmentManager != null)
            {
                ServiceLocator.Register(environmentManager);
            }
        }
    }
}
