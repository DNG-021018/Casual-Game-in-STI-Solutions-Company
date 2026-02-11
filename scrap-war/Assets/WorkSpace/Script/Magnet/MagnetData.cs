using UnityEngine;

[CreateAssetMenu(fileName = "MagnetData", menuName = "Scrap War/Magnet Data", order = 1)]
public class MagnetData : ScriptableObject
{
    [Header("Magnet Stats")]
    public float pullForce;
    public float shootForce;
}
