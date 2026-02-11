using UnityEngine;

namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_CharacterIdleState : bJakGZQ3_ACharacterState
    {
        float _afkTimer;
        bool _isDancing;

        public override void Enter(bJakGZQ3_IStateMachine stateMachine)
        {
            base.Enter(stateMachine);

            _afkTimer = 0f;
            _isDancing = false;

            float blend = RandomIdleAnimation();
            if (anim != null)
            {
                anim.SetFloat("IdleBlend", Mathf.RoundToInt(blend));
                anim.SetBool("isDancing", false);
            }

            if (gridMovement != null)
            {
                gridMovement.EnableMovement();
            }
        }

        public override void OnUpdateState(bJakGZQ3_IStateMachine stateMachine)
        {
            base.OnUpdateState(stateMachine);

            if (gridMovement != null && gridMovement.IsMoving)
            {
                if (_isDancing && anim != null)
                {
                    anim.SetBool("isDancing", false);
                }
                SwitchState(EntityState.Move);
                return;
            }

            _afkTimer += Time.deltaTime;

            if (!_isDancing && _afkTimer >= characterStateMachine.afkDelay)
            {
                StartDance();
            }

            if (_isDancing && anim != null)
            {
                AnimatorStateInfo st = anim.GetCurrentAnimatorStateInfo(0);

                if (st.IsName("Dancing"))
                {
                    if (st.normalizedTime >= 0.95f)
                    {
                        StopDanceAndReturnToIdle();
                    }
                }
                else
                {
                    if (!st.IsName("Dancing"))
                    {
                        _isDancing = false;
                        _afkTimer = 0f;
                    }
                }
            }
        }

        public override void Exit(bJakGZQ3_IStateMachine stateMachine)
        {
            base.Exit(stateMachine);

            _isDancing = false;
            _afkTimer = 0f;
        }

        public override void OnTriggerEnter(bJakGZQ3_IStateMachine stateMachine, Collider collider)
        {
            base.OnTriggerEnter(stateMachine, collider);
        }

        float RandomIdleAnimation()
        {
            return Random.Range(0f, 2.999f);
        }

        void StartDance()
        {
            if (anim == null) return;

            anim.SetBool("isDancing", true);
            _isDancing = true;
        }

        void StopDanceAndReturnToIdle()
        {
            if (anim == null) return;

            anim.SetBool("isDancing", false);

            _isDancing = false;
            _afkTimer = 0f;

            float blend = RandomIdleAnimation();
            anim.SetFloat("IdleBlend", Mathf.RoundToInt(blend));
        }
    }
}
