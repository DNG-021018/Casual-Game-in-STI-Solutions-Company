namespace Bowmancer
{
    public static class B_SafetyKey
    {
        public const string KEY_GAME_NAME = "Bowmancer";

        // ================ DATA ================
        public const string KEY_PLAYPREF_MAX_UNLOCKED_LEVEL = "MaxUnlockedLevel";
        public const string KEY_PLAYPREF_MUSIC_ON = "MUSIC_ON";
        public const string KEY_PLAYPREF_SFX_ON = "SFX_ON";
        public const string KEY_PLAYPREF_MUSIC_VOLUME = "MUSIC_VOLUME";
        public const string KEY_PLAYPREF_SFX_VOLUME = "SFX_VOLUME";

        // ================ ANIMATION KEY ================
        // Player
        public const string ANIM_PLAYER_BLEND_MOVING = "Blend";
        public const string ANIM_PLAYER_BLEND_SHOOTING_VELOCITY_X = "Velocity X";
        public const string ANIM_PLAYER_BLEND_SHOOTING_VELOCITY_Y = "Velocity Y";
        public const string ANIM_PLAYER_BOOL_SHOOT = "isShooting";
        public const string ANIM_PLAYER_TRIGGER_DEAD = "isDead";

        // Enemy
        public const string ANIM_ENEMY_BLEND_MOVING_BLEND = "MovingBlend";
        public const string ANIM_ENEMY_TRIGGER_DEAD = "isDead";
        public const string ANIM_ENEMY_TRIGGER_ATTACK = "isAttack";
        public const string ANIM_ENEMY_TRIGGER_GETHIT = "getHit";

        //door
        public const string ANIM_DOOR_TRIGGER_OPEN = "Open";
        public const string ANIM_DOOR_TRIGGER_CLOSE = "Close";

        // Volume 
        public const string ANIM_TRIGGER_HIT = "isHit";
        public const string ANIM_BOOL_LOW_HEALTH = "isLowHealth";

        // ================ Daily Reward ================
        public const string DAILY_REWARD_SAVE_KEY = "DailyRewardData";

        // ================ TAG ================
        public const string TAG_PLAYER = "Player";
        public const string TAG_COIN = "Coin";

        // ================ UPGRADE ================
        public const string PERMANENT_UPGRADE_SAVE_KEY = "PermanentUpgrades";
    }
}
