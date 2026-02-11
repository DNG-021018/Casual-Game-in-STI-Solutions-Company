using DG.Tweening;
using UnityEngine;

namespace Wja8YNiR_PrismPath
{
    public class Wja8YNiR_Door : Wja8YNiR_Entities
    {
        [Header("Refs")]
        [SerializeField] private Wja8YNiR_Button sourceButton;
        private Animator animator;
        int OpenHash = Animator.StringToHash("IsOpen");

        private void Start()
        {
            animator = GetComponent<Animator>();
        }

        void OnEnable()
        {
            if (sourceButton != null)
                sourceButton.OnPressedChanged += OpenDoor;
        }

        void OnDisable()
        {
            if (sourceButton != null)
                sourceButton.OnPressedChanged -= OpenDoor;
        }

        void OpenDoor(bool isOpen)
        {
            if (isOpen) Open();
            else Close();
        }

        public void Open()
        {
            animator.SetBool(OpenHash, true);
        }

        public void Close()
        {
            animator.SetBool(OpenHash, false);
        }

    }
}
