using System;
using UnityEngine;

namespace HexaStack
{
    public class HS_Hexagon : MonoBehaviour
    {
        [Header("Elements")]
        [SerializeField] private new Renderer renderer;
        [SerializeField] private new Collider collider;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip moveClip;
        [SerializeField] private AudioClip mergeClip;

        public HS_HexStack HexStack { get; private set; }
        private HS_AudioManager audioManager;

        void Awake()
        {
            audioManager = HS_AudioManager.Instance;
        }

        public Color Color
        {
            get => renderer.material.color;
            set => renderer.material.color = value;
        }

        public void Configure(HS_HexStack hexStack)
        {
            HexStack = hexStack;
        }

        internal void DisableCollider()
        {
            collider.enabled = false;
        }

        public void SetParent(Transform parent)
        {
            transform.SetParent(parent);
        }

        public void MoveToLocal(Vector3 targetLocalPosition)
        {
            LeanTween.cancel(gameObject);

            float delay = transform.GetSiblingIndex() * 0.07f;

            Vector3 midpoint = (transform.localPosition + targetLocalPosition) / 2;
            midpoint.y += 2f;

            LeanTween.moveLocal(gameObject, midpoint, 0.15f)
            .setEase(LeanTweenType.easeOutQuad)
            .setDelay(delay)
            .setOnComplete(() =>
            {
                LeanTween.moveLocal(gameObject, targetLocalPosition, 0.15f)
                .setEase(LeanTweenType.easeInQuad);
            });

            audioManager.PlaySfx(moveClip);

            Vector3 direction = (targetLocalPosition - transform.localPosition).With(y: 0).normalized;
            Vector3 rotationAxis = Vector3.Cross(Vector3.up, direction);

            LeanTween.rotateAround(gameObject, rotationAxis, 180, 0.3f)
            .setEase(LeanTweenType.easeInOutSine)
            .setDelay(delay);
        }

        public void Vanish(float delay)
        {
            audioManager.PlaySfx(mergeClip);
            LeanTween.scale(gameObject, Vector3.zero, 0.2f)
            .setEase(LeanTweenType.easeInBack)
            .setDelay(delay)
            .setOnComplete(() => Destroy(gameObject));
        }
    }
}
