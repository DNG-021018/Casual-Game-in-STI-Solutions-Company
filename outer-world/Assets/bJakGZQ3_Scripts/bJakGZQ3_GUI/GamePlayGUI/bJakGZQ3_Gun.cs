using UnityEngine;

namespace bJakGZQ3_Outer_World
{
    public class bJakGZQ3_Gun : MonoBehaviour
    {
        [SerializeField] GameObject avalable;
        [SerializeField] GameObject notAvalable;

        void Start()
        {
            GunNotAvalable();
        }

        public void GunAvalable()
        {
            if (avalable && notAvalable)
            {
                avalable.gameObject.SetActive(true);
                notAvalable.gameObject.SetActive(false);
            }
        }

        public void GunNotAvalable()
        {
            if (avalable && notAvalable)
            {
                avalable.gameObject.SetActive(false);
                notAvalable.gameObject.SetActive(true);
            }
        }
    }
}
