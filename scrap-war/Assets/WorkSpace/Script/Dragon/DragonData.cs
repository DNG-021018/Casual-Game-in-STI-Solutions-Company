using UnityEngine;

[CreateAssetMenu(fileName = "DragonData", menuName = "Scrap War/Dragon Data", order = 1)]
public class DragonData : ScriptableObject
{
    [Header("Dragon Property")]
    public float maxHealth;
    public float Speed;
    public float AngularSpeed;
    public float Acceleration;
    public float StoppingDistance;
    public float Radius;
    public float Height;

    [Header("Dragon Flame Thrower Skills ")]
    public float flameDuration = 5f;
    public float skillDuration = 5f;
    public float flameDamage = 10f;
    public float flameRadius = 1f;
    public float minFlameHitBoxLength = 0f;
    public float maxFlameHitBoxLength = 8f;
}
