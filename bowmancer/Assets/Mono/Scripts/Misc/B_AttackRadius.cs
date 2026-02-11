using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bowmancer
{
    [RequireComponent(typeof(SphereCollider))]
    public class B_AttackRadius : MonoBehaviour
    {
        public SphereCollider sphereCollider;
        private List<B_IDamage> Damagesables = new List<B_IDamage>();
        public float Attackdelay = 0.5f;
        private float AttackDamage;
        public delegate void AttackEvent(B_IDamage target);
        public AttackEvent OnAttack;
        private Coroutine AttackCoroutine;

        void Awake()
        {
            sphereCollider = GetComponent<SphereCollider>();
        }

        public void Init(float damage, float attackRadius, float attackDelay)
        {
            AttackDamage = damage;
            sphereCollider.radius = attackRadius;
            Attackdelay = attackDelay;
        }

        private void OnTriggerEnter(Collider other)
        {
            B_AEntity damageable = other.GetComponent<B_AEntity>();
            if (damageable != null && !Damagesables.Contains(damageable))
            {
                Damagesables.Add(damageable);

                if (AttackCoroutine == null)
                {
                    AttackCoroutine = StartCoroutine(Attack());
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            B_AEntity damageable = other.GetComponent<B_AEntity>();
            if (damageable != null && Damagesables.Contains(damageable))
            {
                Damagesables.Remove(damageable);

                if (Damagesables.Count == 0 && AttackCoroutine != null)
                {
                    StopCoroutine(AttackCoroutine);
                    AttackCoroutine = null;
                }
            }
        }

        private IEnumerator Attack()
        {
            WaitForSeconds Wait = new WaitForSeconds(Attackdelay);
            yield return Wait;

            B_IDamage closetDamageable = null;
            float closetDistance = float.MaxValue;

            while (Damagesables.Count > 0)
            {
                foreach (B_IDamage damageable in Damagesables)
                {
                    for (int i = 0; i < Damagesables.Count; i++)
                    {
                        Transform damageableTransform = Damagesables[i].GetTransform();
                        float distance = Vector3.Distance(transform.position, damageableTransform.position);
                        if (distance < closetDistance)
                        {
                            closetDistance = distance;
                            closetDamageable = Damagesables[i];
                        }
                    }
                }

                if (closetDamageable != null)
                {
                    OnAttack?.Invoke(closetDamageable);
                    // closetDamageable.TakeDamage(AttackDamage);
                }

                // closetDamageable = null;
                // closetDistance = float.MaxValue;

                yield return Wait;

                Damagesables.RemoveAll(DisableDamageables);
            }

            AttackCoroutine = null;
        }

        private bool DisableDamageables(B_IDamage damageable)
        {
            return damageable != null && !damageable.GetTransform().gameObject.activeSelf;
        }

        public void ExecuteAttack()
        {
            if (Damagesables.Count == 0) return;

            B_IDamage closetDamageable = null;
            float closetDistance = float.MaxValue;

            foreach (B_IDamage damageable in Damagesables)
            {
                for (int i = 0; i < Damagesables.Count; i++)
                {
                    Transform damageableTransform = Damagesables[i].GetTransform();
                    float distance = Vector3.Distance(transform.position, damageableTransform.position);
                    if (distance < closetDistance)
                    {
                        closetDistance = distance;
                        closetDamageable = Damagesables[i];
                    }
                }
            }

            if (closetDamageable != null)
            {
                closetDamageable.TakeDamage(AttackDamage);
            }

            closetDamageable = null;
            closetDistance = float.MaxValue;

            Damagesables.RemoveAll(DisableDamageables);
        }
    }
}
