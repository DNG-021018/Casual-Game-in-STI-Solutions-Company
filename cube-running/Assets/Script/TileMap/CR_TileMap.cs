using UnityEngine;
using DG.Tweening;

namespace CB_CubeRunner
{
    public enum CR_TileType
    {
        Floor,
        Wall
    }

    public class CR_TileMap : MonoBehaviour
    {
        [Header("Type")]
        public CR_TileType tileType = CR_TileType.Floor;

        [Header("Highlight")]
        [SerializeField] GameObject quad;
        MeshRenderer _quadRenderer;

        Vector3 _startLocalPos;
        Quaternion _startLocalRot;
        Tween _fallTween;

        void Awake()
        {
            if (quad != null)
            {
                _quadRenderer = quad.GetComponent<MeshRenderer>();
            }

            _startLocalPos = transform.localPosition;
            _startLocalRot = transform.localRotation;
        }

        void OnEnable()
        {
            if (quad != null) quad.SetActive(false);
        }

        void OnDisable()
        {
            if (quad != null) quad.SetActive(false);
            _fallTween?.Kill();
        }

        public void SetQuad(bool on, Color color)
        {
            if (quad == null || _quadRenderer == null) return;

            quad.SetActive(on);
            if (_quadRenderer.material.HasProperty("_Color"))
            {
                _quadRenderer.material.color = color;
            }
            else
            {
                _quadRenderer.material.SetColor("_BaseColor", color);
            }
        }

        public void EnableFall(float duration, float fallDistance)
        {
            _fallTween?.Kill();

            float randomDelay = Random.Range(0f, 0.1f);

            Vector3 currentPos = transform.position;
            Vector3 targetPos = currentPos - new Vector3(0f, fallDistance, 0f);

            Sequence fallSequence = DOTween.Sequence();

            fallSequence.AppendInterval(randomDelay);

            fallSequence.Append(
                transform.DOMoveY(targetPos.y, duration)
                    .SetEase(Ease.InCubic)
            );

            fallSequence.Join(
                transform.DORotate(
                    _startLocalRot.eulerAngles + new Vector3(
                        Random.Range(-20f, 20f),
                        Random.Range(-30f, 30f),
                        Random.Range(-20f, 20f)
                    ),
                    duration
                ).SetEase(Ease.InOutQuad)
            );

            _fallTween = fallSequence;
        }

        public void ResetTile()
        {
            _fallTween?.Kill();
            SetQuad(false, default);
            transform.localPosition = _startLocalPos;
            transform.localRotation = _startLocalRot;
        }
    }
}
