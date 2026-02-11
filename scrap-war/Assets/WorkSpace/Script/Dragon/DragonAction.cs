using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class DragonAction : DragonComponents
{
    [SerializeField] DragonFlame dragonFlame;

    [Header("Cooldown Settings")]
    [SerializeField] float flameCooldown = 1f;

    [Header("Debug Sensor Editor Settings")]
    [Tooltip("LayerMask to detect Player")]
    [SerializeField] LayerMask layerMask;
    [SerializeField] float range = 10f;
    [SerializeField] Color gizmoColor = Color.green;
    [SerializeField] Color gizmoColorDetected = Color.red;
    [SerializeField, Range(0f, 360f)] float angle = 90f;

    [Header("Effect Settings")]
    [SerializeField] Color gizmozFlameHitBox = Color.green;

    Coroutine flameCoroutine;
    DragonData dragonData;
    Vector3 flameOrigin;
    Vector3 flameDirection;

    string playerTag = "Player";

    float UpdateSpeed = 0.1f;
    float flameHitBoxLength;

    float cooldownTimer = 0f;
    bool isCooldown = false;
    bool isPlayerDetected = false;

    float defaultSpeed;
    float defaultAngularSpeed;

    public override void Initialize(DragonController dc)
    {
        base.Initialize(dc);

        dragonFlame = dragonController.GetComponentInChildren<DragonFlame>();
        ValidationUtils.CheckNull(dragonFlame, "[DragonAction.cs] ---> cant not find FlameThrowerEffect");

        dragonData = dragonController.dragonData;
        defaultSpeed = dragonController._agent.speed;
        defaultAngularSpeed = dragonController._agent.angularSpeed;
    }

    private IEnumerator Check()
    {
        WaitForSeconds wait = new WaitForSeconds(UpdateSpeed);
        float playerInRangeTime = 0f;
        float requiredTime = 1f;

        while (dragonController.enabled)
        {
            Collider[] colliders = Physics.OverlapSphere(dragonController.transform.position, range, layerMask);
            bool playerFound = false;

            foreach (Collider col in colliders)
            {
                if (col.CompareTag(playerTag))
                {
                    Vector3 dirToPlayer = (col.transform.position - dragonController.transform.position).normalized;
                    float angleToPlayer = Vector3.Angle(dragonController.transform.forward, dirToPlayer);
                    if (angleToPlayer <= angle / 2f)
                    {
                        playerFound = true;
                        break;
                    }
                }
            }

            isPlayerDetected = playerFound;

            if (playerFound)
            {
                playerInRangeTime += UpdateSpeed;

                if (playerInRangeTime >= requiredTime && !isCooldown && flameCoroutine == null)
                {
                    flameCoroutine = dragonController.StartCoroutine(FlameThrower());
                }
            }
            else
            {
                playerInRangeTime = 0f;
                dragonController.ExitCombat();
            }

            if (isCooldown)
            {
                cooldownTimer -= UpdateSpeed;
                if (cooldownTimer <= 0f)
                {
                    isCooldown = false;
                }
            }

            yield return wait;
        }
    }

    public IEnumerator FlameThrower()
    {
        dragonController._agent.isStopped = true;
        dragonController._animator.SetBool("isAttacking", true);
        dragonController._agent.ResetPath();
        dragonController._agent.velocity = Vector3.zero;
        dragonController._agent.nextPosition = dragonController.transform.position;

        dragonController.EnterCombat();
        dragonController.IsAttacking = true;

        dragonController.dragonSound?.PlayRoar();

        yield return new WaitForSeconds(1f);
        yield return new WaitUntil(() => dragonController.canFlame);

        dragonController.SetCanRotate(false); // 🔒 KHÓA XOAY
        dragonFlame.PlayFlame();
        dragonController.dragonSound?.PlayFlameWithLoop();

        float elapsed = 0f;
        flameHitBoxLength = 0f;

        while (elapsed < dragonData.skillDuration)
        {
            float t = Mathf.Clamp01(elapsed / dragonData.flameDuration);
            flameHitBoxLength = Mathf.Lerp(dragonData.minFlameHitBoxLength, dragonData.maxFlameHitBoxLength, t);

            flameOrigin = dragonFlame.transform.position;
            flameDirection = dragonFlame.transform.forward.normalized;

            Vector3 point1 = flameOrigin;
            Vector3 point2 = flameOrigin + flameDirection * flameHitBoxLength;

            Collider[] hits = Physics.OverlapCapsule(point1, point2, dragonData.flameRadius, layerMask);
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag(playerTag) &&
                    hit.TryGetComponent<IDamage<PlayerController>>(out IDamage<PlayerController> dmg))
                {
                    dmg.TakeDamage(dragonData.flameDamage);
                }
            }

            if (elapsed >= dragonData.flameDuration && dragonFlame.gameObject.activeSelf)
            {
                dragonFlame.StopFlame(2f);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        dragonController._animator.SetBool("isAttacking", false);
        dragonController.canFlame = false;

        yield return new WaitUntil(() => dragonController.canGo);
        yield return new WaitForSeconds(0.2f);

        dragonController.IsAttacking = false;
        dragonController._agent.isStopped = false;
        dragonController._agent.speed = defaultSpeed;
        dragonController._agent.angularSpeed = defaultAngularSpeed;
        dragonController.SetCanRotate(true); // ✅ MỞ LẠI XOAY
        dragonController.canGo = false;
        dragonController._agent.ResetPath();

        dragonController.dragonSound?.StopFlameLoop();

        flameHitBoxLength = 0;
        flameCoroutine = null;
        isCooldown = true;
        cooldownTimer = flameCooldown;

        dragonController.ExitCombat();
    }

    private void OnDrawGizmos()
    {
        if (dragonController == null) return;

        Gizmos.color = !Application.isPlaying ? gizmoColor : (isPlayerDetected ? gizmoColorDetected : gizmoColor);
        Vector3 origin = dragonController.transform.position;
        Quaternion baseRot = Quaternion.LookRotation(dragonController.transform.forward);

        int segments = 30;
        float step = angle / segments;
        Vector3 prevPoint = origin + baseRot * Quaternion.Euler(0, -angle / 2f, 0) * Vector3.forward * range;

        for (int i = 1; i <= segments; i++)
        {
            float yaw = -angle / 2f + step * i;
            Vector3 next = origin + baseRot * Quaternion.Euler(0, yaw, 0) * Vector3.forward * range;
            Gizmos.DrawLine(origin, next);
            Gizmos.DrawLine(prevPoint, next);
            prevPoint = next;
        }

        if (Application.isPlaying && dragonFlame != null && flameHitBoxLength > 0)
        {
            Vector3 p1 = flameOrigin;
            Vector3 p2 = flameOrigin + flameDirection * flameHitBoxLength;

            Gizmos.color = gizmozFlameHitBox;
            Gizmos.DrawWireSphere(p1, dragonData.flameRadius);
            Gizmos.DrawWireSphere(p2, dragonData.flameRadius);
            Gizmos.DrawLine(p1 + Vector3.up * dragonData.flameRadius, p2 + Vector3.up * dragonData.flameRadius);
            Gizmos.DrawLine(p1 - Vector3.up * dragonData.flameRadius, p2 - Vector3.up * dragonData.flameRadius);
            Gizmos.DrawLine(p1 + Vector3.right * dragonData.flameRadius, p2 + Vector3.right * dragonData.flameRadius);
            Gizmos.DrawLine(p1 - Vector3.right * dragonData.flameRadius, p2 - Vector3.right * dragonData.flameRadius);
        }
    }

    public override void Start() => dragonController.StartCoroutine(Check());
    public override void Update() { }
    public override void DrawGizmos() => OnDrawGizmos();
}
