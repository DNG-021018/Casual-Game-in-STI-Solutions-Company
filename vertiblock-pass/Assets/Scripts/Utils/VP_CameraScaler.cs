using UnityEngine;

namespace VertiblockPass
{
    public class VP_CameraScaler : MonoBehaviour
    {
        public enum ScaleMode
        {
            FitHeight,
            FitWidth,
            LetterBox,
            Expand
        }

        [Header("Scale Settings")] public ScaleMode scaleMode = ScaleMode.Expand;
        [Header("Camera")] public Camera _Camera;

        public float referenceWidth = 18.48f;
        public float referenceHeight = 40f;

        private void Awake()
        {
            if (_Camera == null)
            {
                _Camera = Camera.main;
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

            _Camera.orthographicSize = targetOrthoSize;
        }

        [ContextMenu("Calculate Reference from Current Lens")]
        private void CalculateReference()
        {
            if (_Camera == null)
            {
                _Camera = Camera.main;
            }

            float screenAspect = (float)Screen.width / Screen.height;
            float orthoSize = _Camera.orthographicSize;

            referenceHeight = orthoSize * 2f;
            referenceWidth = referenceHeight * screenAspect;
        }
    }
}
