using UnityEngine;
using DG.Tweening;

namespace CB_CubeRunner
{
    public class CR_Spike : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform spikeRoot;

        [Header("Pos Y")]
        [SerializeField] private float downY = -0.349f;
        [SerializeField] private float upY = 0.0712f;

        [Header("Time Settings")]
        [SerializeField] private float upDuration = 0.2f;
        [SerializeField] private float downDuration = 0.2f;
        [SerializeField] private float waitOnTop = 0.3f;
        [SerializeField] private float waitOnBottom = 0.3f;

        [Header("Scale Settings")]
        [SerializeField, Range(0f, 1f)] private float downScaleFactor = 0.05f;

        [SerializeField] AudioClip hitSFX;

        Sequence _seq;
        Vector3 _defaultScale;
        CB_AudioManager audioManager;

        void Awake()
        {
            if (spikeRoot == null)
                spikeRoot = transform;

            _defaultScale = spikeRoot.localScale;
        }

        void OnEnable()
        {
            PlaySpikeTween();
        }

        void OnDisable()
        {
            if (_seq != null)
                _seq.Kill();
        }

        private void Start()
        {
            audioManager = CB_AudioManager.Instance;
        }

        public void PlaySpikeTween()
        {
            if (_seq != null)
                _seq.Kill();

            Vector3 startPos = spikeRoot.localPosition;
            startPos.y = downY;
            spikeRoot.localPosition = startPos;

            Vector3 downScale = new Vector3(
                _defaultScale.x,
                _defaultScale.y * downScaleFactor,
                _defaultScale.z
            );

            spikeRoot.localScale = downScale;

            _seq = DOTween.Sequence();

            _seq.Append(spikeRoot
                .DOLocalMoveY(upY, upDuration)
                .SetEase(Ease.OutQuad));
            _seq.Join(spikeRoot
                .DOScaleY(_defaultScale.y, upDuration)
                .SetEase(Ease.OutQuad));

            if (waitOnTop > 0f)
                _seq.AppendInterval(waitOnTop);

            _seq.Append(spikeRoot
                .DOLocalMoveY(downY, downDuration)
                .SetEase(Ease.InQuad));
            _seq.Join(spikeRoot
                .DOScaleY(downScale.y, downDuration)
                .SetEase(Ease.InQuad));

            if (waitOnBottom > 0f)
                _seq.AppendInterval(waitOnBottom);

            _seq.SetLoops(-1, LoopType.Restart);
        }

        void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponent<CR_PlayerController>();
            if (player != null)
            {
                other.gameObject.SetActive(false);

                if (audioManager != null && hitSFX != null)
                    audioManager.PlaySfx(hitSFX);

                CB_GameManager.Instance?.SetState(GameState.FinishGame);
            }
        }
    }
}