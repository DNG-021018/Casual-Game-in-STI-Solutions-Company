using System.Collections.Generic;
using UnityEngine;

namespace CB_CubeRunner
{
    public class CR_PlayerController : MonoBehaviour
    {
        [Header("Skin")]
        [SerializeField] private CR_PlayerSkinConfig config;

        private readonly Dictionary<int, CR_PlayerVisual> _skinInstances = new();
        private readonly Dictionary<int, SkinStruct> _skinDataById = new();

        private SkinStruct _currentSkin;
        private CR_PlayerVisual _currentVisual;

        [Header("Camera Target")]
        [SerializeField] private Transform target;
        public Transform Target => target;

        private CB_CameraManager _cameraManager;

        void Awake()
        {
            _cameraManager = CB_CameraManager.Instance;
            if (_cameraManager != null) _cameraManager.SetTarget(this);

            foreach (var s in config.skinConfig)
            {
                if (s.visual == null) continue;

                var inst = Instantiate(s.visual, this.transform);
                inst.gameObject.SetActive(false);

                _skinInstances[s.ID] = inst;
                _skinDataById[s.ID] = s;
            }

            int skinId;

            if (CB_GameManager.Instance != null)
                skinId = CB_GameManager.Instance.CurrentSkinId;
            else
                skinId = GetDefaultSkinId();

            SetSkin(skinId);
        }

        private void OnEnable()
        {
            if (CB_GameManager.Instance != null) CB_GameManager.Instance.OnSkinChanged += SetSkin;
        }

        void OnDestroy()
        {
            if (CB_GameManager.Instance != null) CB_GameManager.Instance.OnSkinChanged -= SetSkin;
        }

        public void SetSkin(int id)
        {
            if (!_skinInstances.ContainsKey(id)) id = GetDefaultSkinId();

            foreach (var kvp in _skinInstances)
            {
                bool isTarget = kvp.Key == id;
                kvp.Value.gameObject.SetActive(isTarget);

                if (isTarget)
                {
                    _currentVisual = kvp.Value;
                    _currentSkin = _skinDataById[id];
                }
            }
        }

        public SkinStruct GetSkin() => _currentSkin;
        public CR_PlayerVisual GetCurrentVisual() => _currentVisual;

        private int GetDefaultSkinId()
        {
            foreach (var s in config.skinConfig)
            {
                if (s.isDefaultSkin)
                    return s.ID;
            }

            return config.skinConfig.Length > 0 ? config.skinConfig[0].ID : 0;
        }
    }
}
