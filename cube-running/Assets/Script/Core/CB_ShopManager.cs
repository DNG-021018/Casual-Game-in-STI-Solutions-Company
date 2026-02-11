using System.Collections.Generic;
using UnityEngine;

namespace CB_CubeRunner
{
    [DefaultExecutionOrder(-49)]
    public class CB_ShopManager : MonoBehaviour
    {
        public static CB_ShopManager Instance { get; private set; }

        public const int SKIN_PRICE = 100;

        private const string KEY_UNLOCKED_SKINS = "UNLOCKED_SKINS";
        private const string KEY_SELECTED_SKIN = "SELECTED_SKIN";

        [Header("Skin Config")]
        [SerializeField] private CR_PlayerSkinConfig skinConfig;
        public CR_PlayerSkinConfig SkinConfig => skinConfig;

        private readonly HashSet<int> unlockedSkins = new();

        private readonly Dictionary<int, SkinStruct> _skinById = new();

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            BuildSkinLookup();
            LoadUnlockedSkins();
            LoadSelectedSkin();
        }

        void BuildSkinLookup()
        {
            _skinById.Clear();

            if (skinConfig == null || skinConfig.skinConfig == null)
            {
                Debug.LogWarning("[ShopManager] SkinConfig is NULL, check inspector.");
                return;
            }

            foreach (var s in skinConfig.skinConfig)
            {
                if (_skinById.ContainsKey(s.ID))
                {
                    Debug.LogWarning($"[ShopManager] Duplicate skin ID {s.ID} in config.");
                    continue;
                }

                _skinById.Add(s.ID, s);
            }
        }

        int GetDefaultSkinIdFromConfig()
        {
            if (skinConfig == null || skinConfig.skinConfig == null || skinConfig.skinConfig.Length == 0)
                return 0;

            foreach (var s in skinConfig.skinConfig)
            {
                if (s.isDefaultSkin)
                    return s.ID;
            }

            return skinConfig.skinConfig[0].ID;
        }

        void LoadUnlockedSkins()
        {
            unlockedSkins.Clear();

            if (skinConfig != null && skinConfig.skinConfig != null)
            {
                foreach (var s in skinConfig.skinConfig)
                {
                    if (s.isDefaultSkin || s.isUnlock)
                        unlockedSkins.Add(s.ID);
                }
            }

            string savedData = PlayerPrefs.GetString(KEY_UNLOCKED_SKINS, "");
            if (!string.IsNullOrEmpty(savedData))
            {
                string[] ids = savedData.Split(',');
                foreach (string id in ids)
                {
                    if (int.TryParse(id, out int skinId))
                        unlockedSkins.Add(skinId);
                }
            }

            if (unlockedSkins.Count == 0)
            {
                int defaultId = GetDefaultSkinIdFromConfig();
                unlockedSkins.Add(defaultId);
            }
        }

        void SaveUnlockedSkins()
        {
            List<int> dynamicUnlocked = new();

            foreach (int id in unlockedSkins)
            {
                if (!_skinById.TryGetValue(id, out var skin))
                    continue;

                if (skin.isDefaultSkin || skin.isUnlock)
                    continue;

                dynamicUnlocked.Add(id);
            }

            dynamicUnlocked.Sort();
            string dataToSave = string.Join(",", dynamicUnlocked);

            PlayerPrefs.SetString(KEY_UNLOCKED_SKINS, dataToSave);
            PlayerPrefs.Save();

        }

        void LoadSelectedSkin()
        {
            int defaultId = GetDefaultSkinIdFromConfig();
            int savedSkinId = PlayerPrefs.GetInt(KEY_SELECTED_SKIN, defaultId);

            if (!_skinById.ContainsKey(savedSkinId))
                savedSkinId = defaultId;

            if (CB_GameManager.Instance != null)
                CB_GameManager.Instance.SelectSkin(savedSkinId);
        }

        void SaveSelectedSkin(int skinId)
        {
            PlayerPrefs.SetInt(KEY_SELECTED_SKIN, skinId);
            PlayerPrefs.Save();
        }

        public bool IsSkinUnlocked(int skinId)
        {
            return unlockedSkins.Contains(skinId);
        }

        public bool TryBuySkin(int skinId)
        {
            if (!_skinById.ContainsKey(skinId))
            {
                Debug.LogWarning($"[ShopManager] TryBuySkin: skin {skinId} không có trong SkinConfig.");
                return false;
            }

            if (IsSkinUnlocked(skinId))
            {
                return false;
            }

            if (CB_GameManager.Instance == null)
            {
                Debug.LogError("[ShopManager] GameManager not found!");
                return false;
            }

            if (CB_GameManager.Instance.TotalCoin < SKIN_PRICE)
            {
                return false;
            }

            CB_GameManager.Instance.AddCoin(-SKIN_PRICE);

            unlockedSkins.Add(skinId);
            SaveUnlockedSkins();

            return true;
        }

        public void SelectAndApplySkin(int skinId)
        {
            if (!IsSkinUnlocked(skinId))
            {
                Debug.LogWarning($"[ShopManager] Try select locked skin {skinId}");
                return;
            }

            SaveSelectedSkin(skinId);

            if (CB_GameManager.Instance != null)
            {
                CB_GameManager.Instance.SelectSkin(skinId);
            }
        }
    }
}