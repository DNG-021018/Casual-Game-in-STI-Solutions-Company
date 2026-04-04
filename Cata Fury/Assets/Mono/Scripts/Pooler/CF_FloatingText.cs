using _Workspace._Scripts.Core.UtilityCore.PoolingCore;
using UnityEngine;
using DG.Tweening;
using TMPro;

namespace CataFury
{
    public class CF_FloatingText : MonoBehaviour, IPoolableWithInit<CF_FloatingText>
    {
        [SerializeField] TextMeshPro textMeshPro;
        [SerializeField] AudioClip floatingTextClip;

        private CF_AudioManager _audioManager;
        Pooler<CF_FloatingText> pool;
        Sequence seq;
        private Vector3 originScale;

        void Awake()
        {
            originScale = transform.localScale;
            _audioManager = ServiceLocator.Get<CF_AudioManager>();
        }

        public void InitPool(Pooler<CF_FloatingText> pool)
        {
            this.pool = pool;
        }

        public void OnGetFromPool()
        {
            seq?.Kill();
            textMeshPro.alpha = 1f;
            transform.localScale = originScale;
        }

        public void OnReturnToPool()
        {
            seq?.Kill();
            textMeshPro.text = "0";
            pool.ReturnToPool(this);
        }

        public void ShowFloatingText(string text, Transform pos, Color? color = null, bool highLight = false)
        {
            seq?.Kill();

            Vector3 dirToCam = transform.position - Camera.main.transform.position;
            transform.rotation = Quaternion.LookRotation(dirToCam);

            textMeshPro.text = text;
            textMeshPro.color = color ?? textMeshPro.color;
            textMeshPro.alpha = 1f;

            Vector3 startPos = pos.position + Vector3.up * 3f;
            transform.position = startPos;
            transform.localScale = originScale * (highLight ? 1.3f : 1f);

            Vector3 velocity = new(Random.Range(-1.5f, 1.5f), Random.Range(6f, 8f), 0);

            float gravity = -18f;
            float duration = 0.7f;

            float time = 0f;

            seq = DOTween.Sequence();
            if (floatingTextClip != null) _audioManager?.PlaySfx(floatingTextClip);
            seq.Append(DOTween.To(
                () => time,
                t =>
                {
                    float dt = Time.deltaTime;
                    time += dt;

                    velocity.y += gravity * dt;
                    transform.position += velocity * dt;

                    if (velocity.y < 0)
                    {
                        float scale = Mathf.Lerp(
                            transform.localScale.x,
                            0.3f,
                            dt * 6f
                        );
                        transform.localScale = Vector3.one * scale;
                    }
                },
                duration,
                duration
            ));

            seq.Join(textMeshPro.DOFade(0f, 0.25f).SetDelay(0.45f));
            seq.OnComplete(OnReturnToPool);
        }
    }
}
