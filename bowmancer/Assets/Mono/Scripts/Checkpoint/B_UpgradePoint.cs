namespace Bowmancer
{
    public class B_UpgradePoint : B_BaseCheckPoint
    {
        protected override void OnUpgradeActivated()
        {
            base.OnUpgradeActivated();
            _gameManager.SetState(GameState.PickupUpgrade);
        }
    }
}
