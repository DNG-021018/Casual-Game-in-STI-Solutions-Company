using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Beyond The Mirror/Character Data", order = 2)]
public class CharacterData : ScriptableObject
{
    [Header("Movement Settings")]
    public float slideSpeed = 30f;

    [Header("Jump Settings")]
    public float jumpForce = 4.5f;
    public float gravity = -9.81f;

    [Header("Crouch Settings")]
    public float crouchDuration = 1.15f;

    [Header("Skill Stats")]
    public float immortalDuration = 5f;
    public Color colorDuration = Color.red;
}
