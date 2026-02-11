using UnityEngine;

namespace Bowmancer
{
    public class B_GoalPoint : B_BaseCheckPoint
    {
        [SerializeField] private Animator animator;

        private int openDoorHash = Animator.StringToHash(B_SafetyKey.ANIM_DOOR_TRIGGER_OPEN);

        public void TriggerOpenDoor()
        {
            animator.SetTrigger(openDoorHash);
        }

        protected override void OnUpgradeActivated()
        {
            base.OnUpgradeActivated();
            TriggerOpenDoor();
            _gameManager.FinishGame(GameState.Win);
        }
    }
}
