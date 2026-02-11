using System;
using System.Collections;
using UnityEngine;
namespace bJakGZQ3_Outer_World
{
    public abstract class bJakGZQ3_ACharacterState : bJakGZQ3_IState
    {
        protected bJakGZQ3_CharacterStateMachine characterStateMachine;
        protected Animator anim;
        protected bJakGZQ3_GridMovement gridMovement;
        protected bJakGZQ3_Player Player;

        public virtual void Enter(bJakGZQ3_IStateMachine stateMachine)
        {
            characterStateMachine = (bJakGZQ3_CharacterStateMachine)stateMachine;
            anim = characterStateMachine.GetComponent<Animator>();
            gridMovement = characterStateMachine.GridMovement;
            Player = characterStateMachine.Player;
        }

        public virtual void Exit(bJakGZQ3_IStateMachine stateMachine)
        {

        }

        public virtual void OnUpdateState(bJakGZQ3_IStateMachine stateMachine)
        {
        }

        public void SwitchState(EntityState state)
        {
            characterStateMachine.SwitchState(state);
        }

        public virtual void OnTriggerEnter(bJakGZQ3_IStateMachine stateMachine, Collider collider)
        {
            if (collider.gameObject.CompareTag("Enemy"))
            {
                bJakGZQ3_Enemy e = collider.gameObject.GetComponentInParent<bJakGZQ3_Enemy>();
                if (e != null)
                {
                    int gunAmount = Player.GetCurrentGun();

                    Vector3 contactPoint = collider.ClosestPoint(Player.transform.position);

                    if (gunAmount <= 0)
                    {
                        SwitchState(EntityState.Hit);
                        float damage = e.GetEnemyDamage();
                        characterStateMachine.Oxygen.TakeOxygenDamage(damage);
                    }
                    else
                    {
                        Player.PlayAttackEffect(contactPoint);
                        SwitchState(EntityState.Attack);
                        Player.UseGun();
                        e.StateMachine.SwitchState(EntityState.Dead);
                    }
                }
            }
        }

        public IEnumerator WaitForEndAnimation(string name, Action OnComplete)
        {
            yield return new WaitUntil(() =>
            {
                return anim.GetCurrentAnimatorStateInfo(0).IsName(name) && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.98f;
            });

            if (OnComplete != null)
            {
                OnComplete.Invoke();
            }
        }
    }
}
