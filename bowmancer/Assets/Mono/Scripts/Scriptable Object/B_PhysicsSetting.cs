using UnityEngine;

namespace Bowmancer
{
    [CreateAssetMenu(fileName = "New Physics Setting", menuName = B_SafetyKey.KEY_GAME_NAME + "/Settings/New Physics Setting")]
    public class B_PhysicsSetting : ScriptableObject
    {
        [Header("Physics Setting")]
        public float Gravity = -9.81f;
        public float GravityMultiplier = 3f;
    }
}
