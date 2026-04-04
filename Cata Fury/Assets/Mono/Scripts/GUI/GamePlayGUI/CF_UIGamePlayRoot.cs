namespace CataFury
{
    public class CF_UIGamePlayRoot : CF_BaseUI
    {
        private GameState _prevState = GameState.None;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            CF_CameraManager.OnTransitionComplete += OnCameraReady;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            CF_CameraManager.OnTransitionComplete -= OnCameraReady;
        }


        protected override void HandleGameState(GameState s)
        {
            switch (s)
            {
                case GameState.Initialize:
                    CloseAll();
                    break;

                case GameState.Tutorial:
                case GameState.Play:
                    CloseAll();
                    if (_prevState == GameState.Pause)
                        Open(UIPageId.GamePlay);
                    break;

                case GameState.Ready:
                    CloseAll();
                    if (_prevState == GameState.Pause)
                        Open(UIPageId.Mainmenu);
                    break;

                case GameState.Pause:
                    Open(UIPageId.PauseMenu);
                    break;

                case GameState.Lose:
                    CloseAll();
                    Open(UIPageId.LoseGame);
                    break;

                case GameState.Cleanup:
                    CloseAll();
                    break;
            }

            _prevState = s;
        }

        private void OnCameraReady(GameState state)
        {
            switch (state)
            {
                case GameState.Tutorial:
                    Open(UIPageId.Tutorial);
                    break;

                case GameState.Play:
                    Open(UIPageId.GamePlay);
                    break;

                case GameState.Ready:
                    Open(UIPageId.Mainmenu);
                    break;
            }
        }
    }
}