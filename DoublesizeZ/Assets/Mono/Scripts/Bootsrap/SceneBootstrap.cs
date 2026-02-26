using UnityEngine;

namespace DoublesideZ
{
    [DefaultExecutionOrder(-10)]
    public class SceneBootstrap : MonoBehaviour
    {
        [Header("Scene Services to Register")]
        [SerializeField] DZ_PopupManager popupManager;
        [SerializeField] DZ_CameraManager cameraManager;
        [SerializeField] DZ_PlayerController playerController;
        [SerializeField] DZ_SpawnManager spawnManager;
        [SerializeField] DZ_PoolManager poolManager;
        [SerializeField] DZ_ScoreManager scoreManager;

        void Awake()
        {
            RegisterServices();
        }

        void Start()
        {
            DZ_LoadingScreenManager.Instance.SetManualProgress(1f);
            DZ_LoadingScreenManager.Instance.HideVisual();
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
                cameraManager.SetTarget(playerController != null ? playerController.transform : null);
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
        }
    }
}
