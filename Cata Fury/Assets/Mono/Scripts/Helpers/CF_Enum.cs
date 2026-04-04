public enum GameState
{
    None,
    Initialize,
    Ready,
    Play,
    Lose,
    Cleanup,
    Pause,
    Tutorial
}

public enum UIPageId
{
    Mainmenu = 0,
    GamePlay = 1,
    LoseGame = 2,
    DailyReward = 3,
    ShopMenu = 5,
    SettingsMenu = 6,
    PauseMenu = 7,
    Tutorial = 8
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

public enum ShopItemType
{
    Default,
    GobWizard,
    LLowyWizard,
    DarkWizard,
    LightWizard,
    ElfWizard,
    CuteWizard,
    LightingWizard,
    lavaWizard,
}

public enum PlayerDirection
{
    Left,
    Right,
    Up,
    Down
}
