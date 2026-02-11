using System;
using UnityEngine;

namespace Wja8YNiR_PrismPath
{
    public class Wja8YNiR_Button : Wja8YNiR_Entities
    {
        public event Action<bool> OnPressedChanged;

        private Animator _animator;
        private bool _isPress = false;
        private GameObject _currentMirror;

        void Start()
        {
            _animator = GetComponent<Animator>();
            Wja8YNiR_Mirror.OnMirrorDestroyed += HandleMirrorDestroyed;
        }

        void OnDestroy()
        {
            Wja8YNiR_Mirror.OnMirrorDestroyed -= HandleMirrorDestroyed;
        }

        private void HandleMirrorDestroyed(Wja8YNiR_Mirror mirror)
        {
            if (_currentMirror != null && _currentMirror == mirror.gameObject)
            {
                _currentMirror = null;
                PressButton(false);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetComponent<Wja8YNiR_Mirror>() != null)
            {
                _currentMirror = other.gameObject;
                PressButton(true);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (_currentMirror != null && other.gameObject == _currentMirror)
            {
                _currentMirror = null;
                Debug.Log("call???");
                PressButton(false);
            }
        }

        public void PressButton(bool isPress)
        {
            if (_isPress == isPress) return;

            _isPress = isPress;

            if (_animator != null)
            {
                _animator.SetBool("TriggerButton", _isPress);
            }

            OnPressedChanged?.Invoke(_isPress);

        }
    }
}
