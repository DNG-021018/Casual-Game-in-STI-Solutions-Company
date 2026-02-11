using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_DataManager : MonoBehaviour
    {
        public static bJakGZQ3_DataManager Instance { get; private set; }

        [Header("Mission Config")]
        [SerializeField] private bJakGZQ3_ItemDatabase itemDatabase;

        [SerializeField] private int minSlotsPerRound = 4;
        [SerializeField] private int maxSlotsPerRound = 9;

        private List<bJakGZQ3_Item> _slots = new List<bJakGZQ3_Item>();
        public IReadOnlyList<bJakGZQ3_Item> Slots => _slots;

        public event Action OnMissionListChanged;
        public event Action<int, bJakGZQ3_Item> OnMissionSlotUpdated;
        public event Action<int> OnGunChanged;

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
        }

        public void GenerateNewRoundMissions()
        {
            _slots.Clear();

            int slotEachRound = Random.Range(minSlotsPerRound, maxSlotsPerRound + 1);

            List<ItemType> availableTypes = new List<ItemType>
            {
                ItemType.FOOD,
                ItemType.ROCKET,
                ItemType.AIDKIT,
                ItemType.DIAMOND
            };

            List<ItemType> selectedTypes = new List<ItemType>();
            ItemType? lastType = null;

            for (int i = 0; i < slotEachRound; i++)
            {
                ItemType nextType = GetRandomTypeNotSameAsLast(availableTypes, lastType);
                selectedTypes.Add(nextType);
                lastType = nextType;
            }

            foreach (var itemType in selectedTypes)
            {
                Sprite icon = itemDatabase != null ? itemDatabase.GetIcon(itemType) : null;
                int requiredAmount = GetRequiredAmount(itemType);

                var slot = new bJakGZQ3_Item(itemType, icon, requiredAmount);
                _slots.Add(slot);
            }

            OnMissionListChanged?.Invoke();
        }

        ItemType GetRandomTypeNotSameAsLast(List<ItemType> available, ItemType? lastType)
        {
            if (available == null || available.Count == 0)
            {
                return ItemType.FOOD;
            }

            if (available.Count == 1)
            {
                return available[0];
            }

            ItemType chosen = available[0];
            int safeCount = 0;

            do
            {
                chosen = available[Random.Range(0, available.Count)];
                safeCount++;

                if (safeCount > 20) break;
            }
            while (lastType.HasValue && chosen == lastType.Value);

            return chosen;
        }

        int GetRequiredAmount(ItemType type)
        {
            if (itemDatabase == null) return 3;

            foreach (var entry in itemDatabase.entries)
            {
                if (entry.itemType == type)
                {
                    return entry.GetRandomRequire();
                }
            }

            return 3;
        }

        public void OnPlayerCollectItem(ItemType type)
        {
            if (_slots == null || _slots.Count == 0) return;

            int currentIndex = GetFirstIncompleteIndex();
            if (currentIndex < 0) return;

            var currentSlot = _slots[currentIndex];

            if (currentSlot.type != type)
            {
                return;
            }

            currentSlot.AddOne();

            OnMissionSlotUpdated?.Invoke(currentIndex, currentSlot);

            if (CheckAllComplete())
            {
                HandleRoundComplete();
            }
        }

        int GetFirstIncompleteIndex()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (!_slots[i].IsComplete)
                    return i;
            }
            return -1;
        }

        bool CheckAllComplete()
        {
            foreach (var slot in _slots)
            {
                if (!slot.IsComplete)
                    return false;
            }
            return true;
        }

        void HandleRoundComplete()
        {
            var lvl = bJakGZQ3_LevelManager.Instance;
            if (lvl != null)
            {
                lvl.NextRound();
            }

            GenerateNewRoundMissions();
        }

        public void OnPlayerGun(int currentGun)
        {
            OnGunChanged?.Invoke(currentGun);
        }
    }
}
