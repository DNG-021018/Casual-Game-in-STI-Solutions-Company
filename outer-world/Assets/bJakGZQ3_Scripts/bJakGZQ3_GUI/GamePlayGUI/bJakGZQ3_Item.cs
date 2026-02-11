using UnityEngine;

namespace bJakGZQ3_Outer_World
{
    [System.Serializable]
    public class bJakGZQ3_Item
    {
        public ItemType type;
        public Sprite icon;

        public int requiredAmount;
        public int currentAmount;

        public bool IsComplete => currentAmount >= requiredAmount;

        public bJakGZQ3_Item(ItemType t, Sprite i, int req)
        {
            type = t;
            icon = i;
            requiredAmount = req;
            currentAmount = 0;
        }

        public void AddOne()
        {
            currentAmount++;
            if (currentAmount > requiredAmount)
                currentAmount = requiredAmount;
        }
    }
}
