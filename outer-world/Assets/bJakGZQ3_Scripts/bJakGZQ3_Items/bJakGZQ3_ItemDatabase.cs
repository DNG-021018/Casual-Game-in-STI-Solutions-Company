using UnityEngine;

namespace bJakGZQ3_Outer_World
{
    [CreateAssetMenu(fileName = "Item Database", menuName = "Outer World/Items/Item Database")]
    public class bJakGZQ3_ItemDatabase : ScriptableObject
    {
        [SerializeField] public bJakGZQ3_ItemPickup[] entries;

        public Sprite GetIcon(ItemType t)
        {
            foreach (var e in entries)
            {
                if (e.itemType == t && e.itemIcon != null)
                    return e.itemIcon;
            }
            return null;
        }

        public int GetRequire(ItemType t)
        {
            foreach (var e in entries)
            {
                if (e.itemType == t)
                {
                    return e.GetRandomRequire();
                }
            }
            return 3; // Default
        }

        public bJakGZQ3_ItemPickup GetEntry(ItemType t)
        {
            foreach (var e in entries)
            {
                if (e.itemType == t)
                    return e;
            }
            return null;
        }
    }
}