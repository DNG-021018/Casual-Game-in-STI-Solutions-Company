using System;
using System.Collections;
using UnityEngine;

namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_AEnemyState : bJakGZQ3_IState
    {
        protected bJakGZQ3_EnemyStateMachine characterStateMachine;
        protected Animator anim;
        protected bJakGZQ3_GridMovement gridMovement;
        protected bJakGZQ3_Enemy Enemy;
        protected bJakGZQ3_GameManager GameManager;

        public virtual void Enter(bJakGZQ3_IStateMachine stateMachine)
        {
            characterStateMachine = (bJakGZQ3_EnemyStateMachine)stateMachine;
            anim = characterStateMachine.GetComponent<Animator>();
            gridMovement = characterStateMachine.GridMovement;
            Enemy = characterStateMachine.Enemy;
            GameManager = characterStateMachine.GameManager;
        }

        public virtual void Exit(bJakGZQ3_IStateMachine stateMachine)
        {

        }

        public virtual void OnUpdateState(bJakGZQ3_IStateMachine stateMachine)
        {
            if (GameManager.GetState() == GameState.FinishGame)
            {
                SwitchState(EntityState.Victory);
                return;
            }
        }

        public void SwitchState(EntityState state)
        {
            characterStateMachine.SwitchState(state);
        }

        public virtual void OnTriggerEnter(bJakGZQ3_IStateMachine stateMachine, Collider collider)
        {
            if (collider.TryGetComponent(out bJakGZQ3_Player p))
            {
                // compute contact point (closest to enemy)
                Vector3 contactPoint = collider.ClosestPoint(Enemy.transform.position);

                if (p.GetCurrentGun() <= 0)
                {
                    // enemy attack VFX
                    Enemy.PlayAttackEffect(contactPoint);
                    SwitchState(EntityState.Attack);
                }
                else
                {
                    // enemy dies VFX
                    Enemy.PlayDeadEffect(contactPoint);
                    SwitchState(EntityState.Dead);
                }
            }
        }

        public IEnumerator WaitForEndAnimation(string name, Action OnComplete)
        {
            yield return new WaitUntil(() =>
            {
                return anim.GetCurrentAnimatorStateInfo(0).IsName(name) && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f;
            });

            if (OnComplete != null)
            {
                OnComplete.Invoke();
            }
        }
    }
}
