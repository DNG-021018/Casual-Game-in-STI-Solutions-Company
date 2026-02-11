using UnityEngine;

namespace Bowmancer
{
    [CreateAssetMenu(fileName = "New Entity Setting", menuName = B_SafetyKey.KEY_GAME_NAME + "/Settings/New Entity Setting")]
    public class B_EntitySetting : ScriptableObject
    {
        public B_PhysicsSetting physics;

        [Header("Move Setting")]
        public float health = 100;
        public float attackPower = 10;
        public float MoveSpeed = 5;
        public float RotationSpeed = 10f;

        [Header("Audio Setting")]
        public AudioClip MoveClip;
        public AudioClip AttackClip;
        public AudioClip HitClip;
        public AudioClip DieClip;

        public EntityStats GetStats()
        {
            return new EntityStats
            {
                Health = health,
                AttackPower = attackPower,
                Gravity = physics.Gravity,
                GravityMultiplier = physics.GravityMultiplier,
                MoveSpeed = MoveSpeed,
                RotationSpeed = RotationSpeed,
                MoveClip = MoveClip,
                AttackClip = AttackClip,
                HitClip = HitClip,
                DieClip = DieClip
            };
        }
    }
}

[System.Serializable]
public class EntityStats
{
    //
    public float Health;
    public float AttackPower;
    public float MoveSpeed;
    public float RotationSpeed;

    //
    public float Gravity;
    public float GravityMultiplier;

    //
    public AudioClip MoveClip;
    public AudioClip AttackClip;
    public AudioClip HitClip;
    public AudioClip DieClip;
}
