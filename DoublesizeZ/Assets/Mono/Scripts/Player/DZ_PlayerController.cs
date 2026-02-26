using System;
using UnityEngine;

namespace DoublesideZ
{
    public class DZ_PlayerController : MonoBehaviour
    {
        public bool IsDead { get; set; }

        private DZ_PlayerInput input;
        private DZ_PlayerAnimationController animationController;
        private DZ_GameManager gameManager;

        public static event Action OnPlayerDeath;

        void Awake()
        {
            input = GetComponent<DZ_PlayerInput>();
            animationController = GetComponent<DZ_PlayerAnimationController>();
            gameManager = DZ_GameManager.Instance;
        }

        void Start()
        {
            input.OnTap += HandleTap;
        }

        void OnDisable()
        {
            input.OnTap -= HandleTap;
        }

        private void HandleTap(TapSide side)
        {
            if (gameManager.IsGameFinished() || gameManager.GetState() != GameState.Play || IsDead)
                return;

            if (side == TapSide.Left)
                LookLeft();
            else
                LookRight();
        }

        void LookLeft()
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        void LookRight()
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        public void Attack()
        {
            animationController.PlayAttackAnimation();
        }

        public void Death()
        {
            IsDead = true;
            OnPlayerDeath?.Invoke();
            animationController.PlayDeathAnimation();
            gameManager.FinishGame(GameState.Lose);
        }

        public void ResetPlayer()
        {
            IsDead = false;
            transform.localScale = Vector3.one;
            animationController?.ResetAnimation();
        }
    }
}
