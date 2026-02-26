using System;
using System.Collections.Generic;
using UnityEngine;

namespace DoublesideZ
{
    public class DZ_PlayerWeapon : MonoBehaviour
    {
        [Serializable]
        struct WeaponID
        {
            public WeaponType weaponType;
            public GameObject weapon;
        }

        [SerializeField] private List<WeaponID> weapons;

        private DZ_WeaponManager _weaponManager;
        private WeaponType _currentEquippedWeapon;

        void Awake()
        {
            _weaponManager = ServiceLocator.Get<DZ_WeaponManager>();
        }

        void Start()
        {
            _currentEquippedWeapon = _weaponManager.GetEquipped();
            EquipWeapon(_currentEquippedWeapon);
        }

        void OnEnable()
        {
            _weaponManager.OnWeaponEquipped += EquipWeapon;
            _weaponManager.OnWeaponPreviewed += EquipWeapon;
        }

        void OnDisable()
        {
            _weaponManager.OnWeaponEquipped -= EquipWeapon;
            _weaponManager.OnWeaponPreviewed -= EquipWeapon;
        }

        public void EquipWeapon(WeaponType weaponType)
        {
            foreach (var weaponID in weapons)
            {
                weaponID.weapon.SetActive(weaponID.weaponType == weaponType);
            }
        }
    }
}
