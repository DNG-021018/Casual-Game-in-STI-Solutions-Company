public enum GameState
{
    None,
    Initialize,
    Ready,
    Play,
    Paused,
    Lose,
    Cleanup
}

public enum UIPageId
{
    Mainmenu = 0,
    GamePlay = 1,
    LoseGame = 2,
    DailyReward = 3,
    PauseMenu = 4,
    ShopMenu = 5,
    SettingsMenu = 6,
}

public enum SlideDir
{
    Left,
    Right,
    Up,
    Down,
    None
}

public enum StateDailyReward
{
    Active,
    Inactive,
    AlreadyClaimed
}

public enum WeaponType
{
    Bat,
    Crowbar,
    Pan,
    Shovel,
    Sledgehammer_Color_1,
    Sledgehammer_Color_2,
}

public enum TapSide
{
    Left,
    Right
}
