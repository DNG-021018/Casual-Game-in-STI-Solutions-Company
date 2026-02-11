using UnityEngine;

namespace bJakGZQ3_Outer_World
{

    [CreateAssetMenu(fileName = "New Enemy", menuName = "Outer World/Enemy/New Enemy")]
    public class bJakGZQ3_EnemyConfig : ScriptableObject
    {
        [SerializeField] float damage = 0f;

        public float GetDamage() => damage;
    }
}
