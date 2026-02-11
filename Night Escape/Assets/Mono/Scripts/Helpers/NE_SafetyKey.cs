namespace NightEscape
{
    public static class NE_SafetyKey
    {
        // ================ DATA PlAYPREF KEY ================
        public const string MAX_UNLOCKED_LEVEL_KEY = "MaxUnlockedLevel";
        public const string KEY_MUSIC_ON = "MUSIC_ON";
        public const string KEY_SFX_ON = "SFX_ON";
        public const string KEY_MUSIC_VOLUME = "MUSIC_VOLUME";
        public const string KEY_SFX_VOLUME = "SFX_VOLUME";

        // ================ ANIMATION KEY ================
        // Door
        public const string ANIM_DOOR_TRIGGER_OPEN = "Open";

        // Police
        public const string ANIM_POLICE_TRIGGER_HIT = "isHit";
        public const string ANIM_POLICE_TRIGGER_CAUGHT = "isCaught";

        // Player
        public const string ANIM_PLAYER_BLEND_IDLE_RUN = "Blend";
        public const string ANIM_PLAYER_TRIGGER_REACH_GOAL = "ReachGoal";
        public const string ANIM_PLAYER_TRIGGER_DOG_CATCH = "isGetCaughtByDog";
        public const string ANIM_PLAYER_TRIGGER_GET_CATCH = "isGetCaught";
        public const string ANIM_PLAYER_TRIGGER_GET_SHOCK = "Shocking";

        // ================ OTHER KEYS ================
        public const string KEY_TAG_POLICE = "police";
        public const string KEY_TAG_DOG = "dog";
        public const string KEY_TAG_TRAP = "trap";
        public const string KEY_TAG_KEY = "key";
        public const string KEY_TAG_PLAYER = "Player";
    }
}
