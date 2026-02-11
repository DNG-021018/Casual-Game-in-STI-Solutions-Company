using System.Collections;
using UnityEngine;

public class BlueZone : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float bluezoneDamage = 5f;
    [SerializeField] private float damageInterval = 0.1f;

    private float damageTimer;
    private IDamage<PlayerController> target;
    private bool isInside;

    private Vector3 originalScale;
    private Coroutine shrinkLoopRoutine;

    void Start()
    {
        isInside = true;
        originalScale = transform.localScale;
    }

    void FixedUpdate()
    {
        if (!isInside && target != null)
        {
            damageTimer -= Time.deltaTime;
            if (damageTimer <= 0f)
            {
                target.TakeDamage(bluezoneDamage);
                damageTimer = damageInterval;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (IsInLayerMask(other.gameObject))
        {
            isInside = true;
            target = null;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsInLayerMask(other.gameObject))
        {
            isInside = false;
            if (other.TryGetComponent<IDamage<PlayerController>>(out IDamage<PlayerController> damageable))
            {
                target = damageable;
                damageTimer = 0f;
            }
        }
    }

    private bool IsInLayerMask(GameObject obj)
    {
        return ((1 << obj.layer) & layerMask.value) != 0;
    }

    public void StartShrinkLoop(float shrinkPercent, float shrinkDuration, float intervalSeconds)
    {
        if (shrinkLoopRoutine != null)
            StopCoroutine(shrinkLoopRoutine);

        shrinkLoopRoutine = StartCoroutine(ShrinkLoopCoroutine(shrinkPercent, shrinkDuration, intervalSeconds));
    }

    private IEnumerator ShrinkLoopCoroutine(float shrinkPercent, float shrinkDuration, float interval)
    {
        Vector3 currentScale = transform.localScale;
        Vector3 minScale = new Vector3(0f, currentScale.y, 0f);

        while (true)
        {
            yield return new WaitForSeconds(interval);

            Vector3 startScale = transform.localScale;
            float targetX = Mathf.Max(originalScale.x * shrinkPercent, minScale.x);
            float targetZ = Mathf.Max(originalScale.z * shrinkPercent, minScale.z);
            Vector3 endScale = new Vector3(targetX, startScale.y, targetZ);

            float timer = 0f;
            while (timer < shrinkDuration)
            {
                timer += Time.deltaTime;
                float t = timer / shrinkDuration;
                transform.localScale = Vector3.Lerp(startScale, endScale, t);
                yield return null;
            }

            transform.localScale = endScale;

            originalScale = endScale;

            if (endScale.x <= minScale.x && endScale.z <= minScale.z)
                break;
        }
    }

    public void ShrinkOnce(float shrinkPercent, float duration)
    {
        StartCoroutine(ShrinkCoroutine(shrinkPercent, duration));
    }

    private IEnumerator ShrinkCoroutine(float shrinkPercent, float duration)
    {
        Vector3 startScale = transform.localScale;
        float targetX = startScale.x * shrinkPercent;
        float targetZ = startScale.z * shrinkPercent;
        Vector3 endScale = new Vector3(targetX, startScale.y, targetZ);

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, endScale, timer / duration);
            yield return null;
        }

        transform.localScale = endScale;
    }

}
