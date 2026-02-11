using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterController : MonoBehaviour
{
    [Tooltip("List Points")]
    public Transform[] lanePoints = new Transform[3];

    [Header("Character Stats")]
    public CharacterData stats;
    public ParticleSystem footStepEffect;
    public GameObject shieldEffect;

    private bool isJumping = false;
    private bool isCrouching = false;
    private float verticalVelocity = 0f;
    public bool isPlayerHit { get; private set; }
    private Coroutine moveRoutine;
    private Coroutine invincibleRoutine;

    [Header("Obstacle Detection")]
    public float rayDistance = 8f;
    public float rayHeightOffset = 0.02f;
    public float rayWidth = 0.5f;
    public float rayHeight = 1.0f;
    public LayerMask obstacleLayers;
    public Color detectionRayGizmoColor = Color.red;

    [Header("Death Detection Raycast")]
    public float deathRayDistance = 2f;
    public float deathRayHeightOffset = 0.5f;
    public float deathRayWidth = 0.5f;
    public float deathRayHeight = 1.5f;
    public LayerMask deathRayLayers;
    public Color deathRayGizmoColor = Color.magenta;
    public float jumpRayYOffset = 1.0f;
    public float crouchRayYOffset = 0.5f;

    // state
    private bool isPlayerWin = false;
    private bool isPlayerHitGate = false;
    public bool isInvincible { get; private set; }

    // components
    private Animator animator;
    private List<Material> cachedMaterials = new List<Material>();
    private List<Color> originalColors = new List<Color>();
    private string colorProperty = null;

    // event
    public static event System.Action OnPlayerHitGate;
    public static event System.Action OnAnyPlayerHit;
    public static event System.Action OnAnySkillActivated;
    public static event System.Action OnCollectItem;

    void Start()
    {
        Init();
    }

    private void Init()
    {
        animator = GetComponent<Animator>();

        Renderer[] renderers = shieldEffect.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            foreach (var mat in renderer.materials)
            {
                if (colorProperty == null)
                {
                    for (int i = 0; i < mat.shader.GetPropertyCount(); i++)
                    {
                        if (mat.shader.GetPropertyType(i) == UnityEngine.Rendering.ShaderPropertyType.Color)
                        {
                            colorProperty = mat.shader.GetPropertyName(i);
                            break;
                        }
                    }
                }

                if (colorProperty != null)
                {
                    cachedMaterials.Add(mat);
                    originalColors.Add(mat.GetColor(colorProperty));
                }
            }
        }

        isPlayerWin = false;
        isPlayerHitGate = false;
        shieldEffect.SetActive(false);
    }

    void Update()
    {
        CheckObstacleHitByRay();
    }

    void CheckObstacleHitByRay()
    {
        if (stats == null) return;
        if (isPlayerHit) return;

        Vector3 origin = GetDeathRayOrigin();
        Vector3 halfExtents = new Vector3(deathRayWidth / 2f, deathRayHeight / 2f, 0.1f);
        Quaternion orientation = Quaternion.identity;

        Collider[] hits = Physics.OverlapBox(origin, halfExtents, orientation, deathRayLayers);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("SpawnedObstacle") && !isPlayerWin && !isPlayerHit)
            {
                if (isInvincible)
                {
                    continue;
                }
                ForceKill();
                break;
            }

            if (hit.CompareTag("Item") && !isPlayerWin && !isPlayerHit)
            {
                ForceActiveSkill();
                SoundManager.Instance?.PickupSound();
                Destroy(hit.gameObject);
            }

            if (hit.CompareTag("CheckGate") && !isPlayerHitGate)
            {
                isPlayerHitGate = true;
                OnPlayerHitGate?.Invoke();
                gameObject.SetActive(false);
            }

        }
    }

    public void ForceActiveSkill()
    {
        if (isInvincible) return;
        if (invincibleRoutine != null)
        {
            StopCoroutine(invincibleRoutine);
        }

        invincibleRoutine = StartCoroutine(ActivateInvincibility());
        Debug.Log("over here");

        OnAnySkillActivated?.Invoke();
    }

    private IEnumerator ActivateInvincibility()
    {
        float duration = stats.immortalDuration;
        isInvincible = true;
        shieldEffect.SetActive(true);

        if (colorProperty == null || cachedMaterials.Count == 0)
        {
            Debug.LogWarning("Không thể đổi màu vì không có material hoặc color property.");
            yield break;
        }

        float elapsed = 0f;
        float fadeStart = duration * 2f / 3f;

        while (elapsed < duration)
        {
            if (elapsed >= fadeStart)
            {
                float time = (elapsed - fadeStart) / (duration / 3f);
                for (int i = 0; i < cachedMaterials.Count; i++)
                {
                    Color baseColor = originalColors[i];
                    cachedMaterials[i].SetColor(colorProperty, Color.Lerp(baseColor, stats.colorDuration, time));
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset color
        for (int i = 0; i < cachedMaterials.Count; i++)
        {
            cachedMaterials[i].SetColor(colorProperty, originalColors[i]);
        }

        isInvincible = false;
        shieldEffect.SetActive(false);
    }

    Vector3 GetDeathRayOrigin()
    {
        float offsetY = deathRayHeightOffset;
        if (isJumping) offsetY += jumpRayYOffset;
        if (isCrouching) offsetY -= crouchRayYOffset;
        return transform.position + Vector3.up * offsetY;
    }

    public void ForceKill()
    {
        TriggerDeath();
    }

    void TriggerDeath()
    {
        if (isPlayerHit) return;
        isPlayerHit = true;

        animator.SetTrigger("isDead");
        footStepEffect.Stop();
        this.enabled = false;

        OnAnyPlayerHit?.Invoke();
    }

    public void WinGame() => isPlayerWin = true;

    public void SlideToLane(int lane)
    {
        if (isPlayerHit || isPlayerWin) return;

        int index = Mathf.Clamp(lane + 1, 0, 2);
        float targetX = lanePoints[index].position.x;

        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(SlideX(targetX));
    }

    private IEnumerator SlideX(float targetX)
    {
        while (Mathf.Abs(transform.position.x - targetX) > 0.01f)
        {
            float newX = Mathf.MoveTowards(transform.position.x, targetX, stats.slideSpeed * Time.deltaTime);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
            yield return null;
        }
        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
    }

    public int GetCurrentLane()
    {
        float currentX = transform.position.x;
        float closestDistance = float.MaxValue;
        int closestLane = 0;

        for (int i = 0; i < lanePoints.Length; i++)
        {
            float dist = Mathf.Abs(currentX - lanePoints[i].position.x);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestLane = i - 1;
            }
        }
        return closestLane;
    }

    public bool CanSlideTo(int targetLane)
    {
        int dir = targetLane - GetCurrentLane();
        if (dir == 0) return true;

        Vector3 origin = transform.position + Vector3.up * rayHeightOffset;
        Vector3 direction = dir < 0 ? Vector3.left : Vector3.right;
        Vector3 halfExtents = new Vector3(rayWidth / 2f, rayHeight / 2f, 0.1f);

        return !Physics.BoxCast(origin, halfExtents, direction, Quaternion.identity, rayDistance, obstacleLayers);
    }

    public void Jump()
    {
        if (isPlayerHit || isPlayerWin || isJumping) return;

        isJumping = true;
        verticalVelocity = stats.jumpForce;
        StartCoroutine(HandleJump());
    }

    private IEnumerator HandleJump()
    {
        animator.SetBool("isSliding", false);
        footStepEffect.Stop();
        animator.SetBool("isJumping", true);

        while (true)
        {
            verticalVelocity += stats.gravity * Time.deltaTime;
            float newY = transform.position.y + verticalVelocity * Time.deltaTime;
            if (newY <= 0f)
            {
                newY = 0f;
                break;
            }
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            yield return null;
        }

        transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
        isJumping = false;
        animator.SetBool("isJumping", false);
        footStepEffect.Play();
    }

    public void CrouchSlide()
    {
        if (isPlayerHit || isPlayerWin || isCrouching) return;

        if (isJumping) verticalVelocity = stats.gravity * 3f;
        StartCoroutine(HandleCrouch());
    }

    private IEnumerator HandleCrouch()
    {
        isCrouching = true;
        animator.SetBool("isSliding", true);
        footStepEffect.Stop();

        yield return new WaitForSeconds(stats.crouchDuration);

        isCrouching = false;
        animator.SetBool("isSliding", false);
        footStepEffect.Play();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = detectionRayGizmoColor;
        Vector3 origin = transform.position + Vector3.up * rayHeightOffset;

        Vector3 size = new Vector3(rayWidth, rayHeight, 0.1f);
        Vector3 leftCenter = origin + Vector3.left * rayDistance;
        Vector3 rightCenter = origin + Vector3.right * rayDistance;

        Gizmos.DrawWireCube(leftCenter, size);
        Gizmos.DrawWireCube(rightCenter, size);

        Gizmos.color = deathRayGizmoColor;

        Vector3 _origin = Application.isPlaying ? GetDeathRayOrigin() : transform.position + Vector3.up * deathRayHeightOffset;
        Vector3 halfExtents = new Vector3(deathRayWidth / 2f, deathRayHeight / 2f, 0.1f);
        Vector3 center = _origin + Vector3.forward * deathRayDistance;
        Gizmos.DrawWireCube(center, halfExtents * 2);
    }
#endif
}
