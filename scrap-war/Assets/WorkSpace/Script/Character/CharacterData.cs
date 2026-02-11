using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Scrap War/Character Data", order = 1)]
public class CharacterData : ScriptableObject
{
    [Header("Character Stats")]
    public float maxHealth;
    public float runSpeed;
    public float turnSmoothTime;
    public float gravity;
}
