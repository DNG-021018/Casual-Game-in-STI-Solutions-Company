using System.Collections;
using UnityEngine;

public class ScapItems : MonoBehaviour, IAttractable, IPoolObject
{
    [SerializeField] private ScrapItemsData scrapItemsData;
    [SerializeField] private GameObject meshObject;
    [SerializeField] private GameObject hitEffectPrefab;

    private AudioSource audioSource;
    [SerializeField] private AudioClip explodeClip;
    [SerializeField, Range(0f, 1f)] private float explodeVolume = 1f;
    [SerializeField] private AudioClip snapClip;
    [SerializeField, Range(0f, 1f)] private float snapVolume = 1f;
    [SerializeField] private float explodeStartTime = 0f;

    private MeshRenderer meshRenderer;
    [SerializeField] private Color startColor = Color.yellow;
    [SerializeField] private Color warningColor = Color.red;

    [SerializeField] private float effectDuration = 1.5f;
    [SerializeField] private float maxLifetime = 5f;

    private Rigidbody rb;
    private float moveSpeed = 10f;
    private float damage => scrapItemsData.damage;
    private bool isBullet;
    private bool wasShot = false;
    public bool WasShot => wasShot;

    private Coroutine lifetimeCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.mass = scrapItemsData.mass;

        meshRenderer = meshObject.GetComponentInChildren<MeshRenderer>();
        audioSource = GetComponent<AudioSource>();
        meshRenderer.material = new Material(meshRenderer.material);
    }

    [System.Obsolete]
    void OnTriggerEnter(Collider other)
    {
        if (!isBullet) return;

        if (other.CompareTag("Dragon"))
        {
            if (transform.parent != null)
                transform.SetParent(null);

            if (other.TryGetComponent<IDamage<DragonController>>(out IDamage<DragonController> damageable))
            {
                damageable.TakeDamage(damage);
            }

            isBullet = false;
            StopLifetimeCoroutine();
            StartCoroutine(HandleHit());
        }
    }

    [System.Obsolete]
    private IEnumerator HandleHit()
    {
        if (meshObject != null)
        {
            meshObject.SetActive(false);
        }

        PlayExplodeSound();

        if (hitEffectPrefab != null)
        {
            hitEffectPrefab.SetActive(true);
            CameraShake.Instance?.ShakeCamera(10f, 0.7f);

            ParticleSystem ps = hitEffectPrefab.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                yield return new WaitForSeconds(ps.main.duration);
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                hitEffectPrefab.SetActive(false);
            }
            else
            {
                yield return new WaitForSeconds(effectDuration);
                hitEffectPrefab.SetActive(false);
            }
        }
        else
        {
            yield return new WaitForSeconds(effectDuration);
        }

        ResetScapItem();
        ObjectPooler.Instance.ReturnToPool(gameObject);
    }

    private void PlayExplodeSound()
    {
        if (explodeClip != null && audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = explodeClip;
            audioSource.time = Mathf.Clamp(explodeStartTime, 0f, explodeClip.length);
            audioSource.volume = Mathf.Clamp01(explodeVolume);
            audioSource.loop = false;
            audioSource.Play();
        }
    }

    private void PlaySnapSound()
    {
        if (snapClip != null && audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = snapClip;
            audioSource.time = 0f;
            audioSource.volume = Mathf.Clamp01(snapVolume);
            audioSource.loop = false;
            audioSource.Play();
        }
    }

    [System.Obsolete]
    private IEnumerator AutoReturnAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        if (!gameObject.activeInHierarchy) yield break;

        if (wasShot && isBullet)
        {
            isBullet = false;
            StartCoroutine(HandleHit());
        }
    }

    private void StopLifetimeCoroutine()
    {
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }
    }

    [System.Obsolete]
    private void ResetScapItem()
    {
        StopLifetimeCoroutine();

        meshObject?.SetActive(true);
        isBullet = false;
        wasShot = false;

        rb.isKinematic = false;
        rb.useGravity = true;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    [System.Obsolete]
    public void AttrachItems(Transform targetPosition, float force)
    {
        if (targetPosition.childCount > 0 || transform.parent != null) return;

        rb.useGravity = false;

        if (!rb.isKinematic)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        float speed = moveSpeed + force / rb.mass;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition.position, speed * Time.deltaTime);

        if ((transform.position - targetPosition.position).sqrMagnitude < 1f)
        {
            rb.isKinematic = true;
            SnapToTarget(targetPosition);
            PlaySnapSound();
        }
        else
        {
            rb.isKinematic = false;
        }
    }

    private void SnapToTarget(Transform targetPosition)
    {
        Vector3 originalScale = transform.lossyScale;
        transform.SetParent(targetPosition, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        Vector3 parentScale = targetPosition.lossyScale;
        transform.localScale = new Vector3(
            originalScale.x / parentScale.x,
            originalScale.y / parentScale.y,
            originalScale.z / parentScale.z
        );

        isBullet = true;
    }

    [System.Obsolete]
    public void Shoot(Vector3 direction, float force)
    {
        wasShot = true;

        if (transform.parent != null)
            transform.SetParent(null);

        rb.isKinematic = false;
        rb.useGravity = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(direction * force, ForceMode.Impulse);
        transform.SetParent(null);

        StopLifetimeCoroutine();
        lifetimeCoroutine = StartCoroutine(AutoReturnAfterTime(maxLifetime));
        colorCoroutine = StartCoroutine(ChangeColorOverTime(maxLifetime));
    }

    private IEnumerator ChangeColorOverTime(float duration)
    {
        Material mat = meshRenderer.material;
        mat.color = startColor;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            mat.color = Color.Lerp(startColor, warningColor, t / duration);
            yield return null;
        }
    }

    public void OnObjectSpawn()
    {
        if (meshRenderer != null)
            meshRenderer.material.color = startColor;
    }

    private Coroutine colorCoroutine;

    private void OnDisable()
    {
        if (colorCoroutine != null)
        {
            StopCoroutine(colorCoroutine);
            colorCoroutine = null;
        }
    }
}
