using _Workspace._Scripts.Core.UtilityCore.PoolingCore;
using UnityEngine;
using DG.Tweening;
using TMPro;

namespace DoublesideZ
{
    public class DZ_FloatingText : MonoBehaviour, IPoolableWithInit<DZ_FloatingText>
    {
        [SerializeField] TextMeshPro textMeshPro;

        Pooler<DZ_FloatingText> pool;
        Sequence seq;

        public void InitPool(Pooler<DZ_FloatingText> pool)
        {
            this.pool = pool;
        }

        public void OnGetFromPool()
        {
            seq?.Kill();
            textMeshPro.alpha = 1f;
            transform.localScale = Vector3.one;
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

            Vector3 startPos = pos.position + Vector3.up * 0.8f;
            transform.position = startPos;
            transform.localScale = Vector3.one * (highLight ? 1.3f : 1f);

            Vector3 velocity = new(UnityEngine.Random.Range(-1.5f, 1.5f), UnityEngine.Random.Range(4.5f, 6.2f), 0);

            float gravity = -18f;
            float duration = 0.7f;

            float time = 0f;

            seq = DOTween.Sequence();

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
