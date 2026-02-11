using System;
using UnityEngine;

namespace Wja8YNiR_PrismPath
{
    public class Wja8YNiR_Mirror : Wja8YNiR_Entities
    {
        [SerializeField] private GameObject hightLightRenderer;
        [SerializeField] private AudioClip intecractClip;

        public static event Action<Wja8YNiR_Mirror> OnMirrorSelected = delegate { };
        public static event Action<Wja8YNiR_Mirror> OnMirrorDestroyed;

        public static Wja8YNiR_Mirror Current { get; private set; }
        Wja8YNiR_LevelManager levelManager;

        void Awake()
        {
            transform.eulerAngles = default;
            if (hightLightRenderer != null) hightLightRenderer.SetActive(false);
        }

        void OnDisable()
        {
            if (Current == this) Current = null;
        }

        void OnDestroy()
        {
            OnMirrorDestroyed?.Invoke(this);
        }

        void Start()
        {
            levelManager = Wja8YNiR_LevelManager.Instance;
        }

        public void Interact()
        {
            if (levelManager?.isGameFinish == true) return;
            Wja8YNiR_GameManager.Instance?.SetState(GameState.Setup);
            Wja8YNiR_AudioManager.Instance.PlaySfx(intecractClip);

            if (Current != null && Current != this)
                Current.UnHightLight();

            Current = this;
            HightLight();

            OnMirrorSelected.Invoke(this);
        }

        public void Rotate45Degrees()
        {
            Vector3 e = transform.eulerAngles;
            e.y += 45f;
            transform.eulerAngles = e;
        }

        public void HightLight()
        {
            if (hightLightRenderer != null) hightLightRenderer.SetActive(true);
        }

        public void UnHightLight()
        {
            if (hightLightRenderer != null) hightLightRenderer.SetActive(false);
            if (Current == this) Current = null;
        }

        public static void ClearCurrentHighlight()
        {
            if (Current != null)
            {
                Current.UnHightLight();
                Current = null;
            }
            if (Wja8YNiR_GameManager.Instance.GetState() == GameState.Setup)
            {
                Wja8YNiR_GameManager.Instance.SetState(GameState.Playing);
            }
        }
    }
}
