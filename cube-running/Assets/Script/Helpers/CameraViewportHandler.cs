using UnityEngine;
using Unity.Cinemachine;

namespace CB_CubeRunner
{
    public class CameraViewportHandler : MonoBehaviour
    {
        public enum ScaleMode
        {
            FitHeight,
            FitWidth,
            LetterBox,
            Expand
        }

        [Header("Scale Settings")] public ScaleMode scaleMode = ScaleMode.Expand;
        [Header("Cinemachine")] public CinemachineCamera cinemachineCamera;

        // Default Reference Device (iPhone 12 Pro Max)
        public float referenceWidth = 18.48f;
        public float referenceHeight = 40f;
        // ===================================

        private float currentWidth;
        private float currentHeight;

        private void Awake()
        {
            if (cinemachineCamera == null)
            {
                cinemachineCamera = GetComponent<CinemachineCamera>();
            }
        }

        private void Start()
        {
            UpdateLens();
        }

        private void UpdateLens()
        {
            float screenAspect = (float)Screen.width / Screen.height;
            float referenceAspect = referenceWidth / referenceHeight;

            float targetOrthoSize = 0f;

            switch (scaleMode)
            {
                case ScaleMode.FitHeight:


                    targetOrthoSize = referenceHeight / 2f;
                    break;

                case ScaleMode.FitWidth:


                    targetOrthoSize = (referenceWidth / 2f) / screenAspect;
                    break;

                case ScaleMode.LetterBox:

                    if (screenAspect > referenceAspect)
                    {

                        targetOrthoSize = referenceHeight / 2f;
                    }
                    else
                    {

                        targetOrthoSize = (referenceWidth / 2f) / screenAspect;
                    }
                    break;

                case ScaleMode.Expand:


                    float orthoSizeByWidth = (referenceWidth / 2f) / screenAspect;
                    float orthoSizeByHeight = referenceHeight / 2f;


                    targetOrthoSize = Mathf.Max(orthoSizeByWidth, orthoSizeByHeight);
                    break;
            }

            cinemachineCamera.Lens.OrthographicSize = targetOrthoSize;
        }

        [ContextMenu("Calculate Reference from Current Lens")]
        private void CalculateReference()
        {
            if (cinemachineCamera == null)
            {
                cinemachineCamera = GetComponent<CinemachineCamera>();
            }

            float screenAspect = (float)Screen.width / Screen.height;
            float orthoSize = cinemachineCamera.Lens.OrthographicSize;

            referenceHeight = orthoSize * 2f;
            referenceWidth = referenceHeight * screenAspect;
        }
    }
}
