using System.Collections.Generic;
using UnityEngine;

namespace CataFury
{
    public class CF_PlayerSkin : MonoBehaviour
    {
        [System.Serializable]
        public struct SkinEntry
        {
            public ShopItemType itemType;
            public GameObject skinObject;

            [Header("Projectile Color")]
            public Color projectileColor;
        }

        [SerializeField] private List<SkinEntry> skins;
        [SerializeField] private Transform skinRoot;

        private CF_ShopManager _shopManager;
        private CF_PlayerController _playerController;
        private GameObject _currentSkinInstance;

        void Awake()
        {
            _shopManager = ServiceLocator.Get<CF_ShopManager>();
            _playerController = ServiceLocator.Get<CF_PlayerController>();

            if (skinRoot == null) skinRoot = transform;
        }

        void OnEnable() => _shopManager.OnItemEquipped += ApplySkin;
        void OnDisable() => _shopManager.OnItemEquipped -= ApplySkin;

        void Start() => ApplySkin(_shopManager.GetEquipped());

        private void ApplySkin(ShopItemType equippedType)
        {
            if (_currentSkinInstance != null)
            {
                Destroy(_currentSkinInstance);
                _currentSkinInstance = null;
            }

            foreach (var entry in skins)
            {
                if (entry.itemType != equippedType) continue;
                if (entry.skinObject == null) continue;

                _currentSkinInstance = Instantiate(
                    entry.skinObject,
                    skinRoot.position,
                    skinRoot.rotation,
                    skinRoot
                );

                Animator anim = _currentSkinInstance.GetComponentInChildren<Animator>(true);
                _playerController?.SetAnimator(anim);

                _playerController?.SetProjectileColor(entry.projectileColor);

                break;
            }
        }
    }
}