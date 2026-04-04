namespace CataFury
{
    public static class CF_SafetyKey
    {
        public const string KEY_GAME_NAME = "CataFury";

        public static class Data
        {
            public const string KEY_PLAYPREF_MAX_UNLOCKED_LEVEL = "MaxUnlockedLevel";
            public const string KEY_PLAYPREF_MUSIC_ON = "MUSIC_ON";
            public const string KEY_PLAYPREF_SFX_ON = "SFX_ON";
            public const string KEY_PLAYPREF_MUSIC_VOLUME = "MUSIC_VOLUME";
            public const string KEY_PLAYPREF_SFX_VOLUME = "SFX_VOLUME";
            public const string DAILY_REWARD_SAVE_KEY = "DailyRewardData";
            public const string DAILY_HIGH_SCORE_SAVE_KEY = "HighScoreData";
            public const string COIN_SAVE_KEY = "PlayerCurrency";
            public const string SHOP_SAVE_KEY = "SHOP_SAVE";
            public const string KEY_TUTORIAL_DONE = "TutorialDone";
        }

        public static class Animation
        {
            public const string ANIM_TRIGGER_ATTACK = "isAttack";
            public const string ANIM_TRIGGER_DEAD = "isDead";
            public const string ANIM_TRIGGER_WIN = "isWin";
        }

        public static class Tag
        {
            public const string TAG_PLAYER = "Player";
            public const string TAG_ENEMY = "Enemy";
        }
    }
}