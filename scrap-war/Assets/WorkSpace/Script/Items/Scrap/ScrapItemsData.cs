using UnityEngine;

[CreateAssetMenu(fileName = "ScrapItemsData", menuName = "Scrap War/Scrap Items Data", order = 1)]
public class ScrapItemsData : ScriptableObject
{
    [Header("Items Data")]
    public float mass;
    public float damage;
}
