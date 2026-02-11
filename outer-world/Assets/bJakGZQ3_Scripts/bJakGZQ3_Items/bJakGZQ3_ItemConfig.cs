using UnityEngine;

namespace bJakGZQ3_Outer_World
{
    [CreateAssetMenu(fileName = "New Item Config", menuName = "Outer World/Items/New Item Config")]
    public class bJakGZQ3_ItemConfig : ScriptableObject
    {
        public ItemType itemType;
        public int MinItemRequire = 2;
        public int MaxItemRequire = 5;
        public int OxyBonus = 8;
        public Sprite itemIcon;
        public AudioClip[] clip;
        public AudioClip monsterClip;
    }
}
