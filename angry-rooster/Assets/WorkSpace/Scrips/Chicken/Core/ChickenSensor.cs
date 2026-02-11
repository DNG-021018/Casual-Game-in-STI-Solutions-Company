using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickenSensor : MonoBehaviour
{
    [Header("Sensor Settings")]
    [Tooltip("LayerMask to detect Player")]
    [SerializeField] LayerMask layerMask;
    [SerializeField] float range = 10f;
    [SerializeField, Range(0f, 360f)] private float angle = 90f;

    [Header("Jump Settings")]
    private IChicken chicken;

    [Header("Effect Settings")]
    [SerializeField] private ParticleSystem jumpEffect;
    [SerializeField] private ParticleSystem roadEffect;

    Transform playerDetected;
    private bool isPlayerDetected;
    private float updateSpeed = 0.1f;
    private bool hasJumped = false;
    private bool hasSlowed = false;

    void Awake()
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        if ((layerMask.value & (1 << playerLayer)) == 0)
        {
            Debug.LogError("[ChickenSensor.cs] ---> LayerMask must include \"Player\" layer");
            return;
        }

        chicken = GetComponent<IChicken>();
        ValidationUtils.CheckNull(chicken, "[ChickenSensor.cs] ---> IChicken is null");
    }

    private void Start()
    {
        StartCoroutine(Check());
    }

    private IEnumerator Check()
    {
        WaitForSeconds Wait = new WaitForSeconds(updateSpeed);

        while (enabled)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, range, layerMask);

            if (colliders.Length > 0)
            {
                Transform player = colliders[0].transform;

                if (colliders[0].CompareTag("Player"))
                {
                    isPlayerDetected = true;
                    playerDetected = player;
                    PerformRandomAction(player, chicken);
                }
            }
            else
            {
                isPlayerDetected = false;
                playerDetected = null;
            }

            yield return Wait;
        }
    }

    private bool isPerformingAction = false;

    private void PerformRandomAction(Transform player, IChicken chicken)
    {
        if (isPerformingAction) return;
        List<Action> possibleActions = new List<Action>();

        if (!hasJumped)
            possibleActions.Add(() => StartCoroutine(JumpToPlayer(player, chicken.JumpCountDownTimer, jumpEffect)));

        if (!hasSlowed)
            possibleActions.Add(() => StartCoroutine(SlowDownPlayer(player, chicken.HonkCountDownTimer, roadEffect)));

        if (possibleActions.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, possibleActions.Count);
            isPerformingAction = true;
            possibleActions[randomIndex].Invoke();
        }
    }

    IEnumerator JumpToPlayer(Transform player, float countdown, ParticleSystem effect)
    {
        hasJumped = true;
        float originalSpeed = chicken.Agent.speed;
        chicken.Agent.speed = 0;

        effect.gameObject.SetActive(true);
        if (!effect.isPlaying) effect.Play();

        AudioSource soundEffect = effect.GetComponent<AudioSource>();

        yield return new WaitForSeconds(effect.main.duration);
        chicken.Animator.SetBool("isCrouch", true);

        yield return new WaitForSeconds(1f);
        chicken.Animator.SetBool("isCrouch", false);
        chicken.Animator.SetBool("isJumping", true);
        soundEffect.Play();

        chicken.Agent.speed = 0;

        Vector3 startPos = transform.position;
        Vector3 targetPos = player.position;
        Vector3 direction = (targetPos - startPos).normalized;

        float distance = Vector3.Distance(startPos, targetPos);
        float maxRange = range;
        float jumpFactor = Mathf.Clamp01(distance / maxRange);

        Vector3 jumpTarget = startPos + direction * (chicken.JumpSpeed * jumpFactor);

        float elapsedTime = 0;

        while (elapsedTime < chicken.JumpDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / chicken.JumpDuration;
            float height = Mathf.Sin(normalizedTime * Mathf.PI) * chicken.JumpHeight;

            Vector3 currentPos = Vector3.Lerp(startPos, jumpTarget, normalizedTime);
            currentPos.y = startPos.y + height;
            transform.position = currentPos;

            yield return null;
        }

        if (effect != null)
        {
            yield return new WaitUntil(() => !effect.isPlaying);
            effect.Stop();
        }

        if (effect.isPlaying)
        {
            effect.Stop();
        }

        if (effect.isPlaying)
        {
            effect.Stop();
        }
        effect.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);
        chicken.Agent.speed = originalSpeed;
        chicken.Animator.SetBool("isJumping", false);



        yield return new WaitForSeconds(countdown);
        hasJumped = false;
        isPerformingAction = false;
    }

    IEnumerator SlowDownPlayer(Transform player, float countdown, ParticleSystem effect)
    {
        hasSlowed = true;
        float originalSpeed = chicken.Agent.speed;
        chicken.Agent.speed = 0;
        chicken.Animator.SetBool("isHonking", true);

        IMovementModifier movementModifier = player.GetComponent<IMovementModifier>();
        ValidationUtils.CheckNull(movementModifier, "[ChickenSensor.cs] ---> movementModifier cannot found on Player");

        effect.gameObject.SetActive(true);
        if (!effect.isPlaying)
        {
            effect.Play();
        }

        AudioSource soundEffect = effect.GetComponent<AudioSource>();
        soundEffect.Play();
        soundEffect.pitch = 1.5f;

        yield return new WaitForSeconds(effect.main.duration + 1f);

        chicken.Animator.SetBool("isHonking", false);
        chicken.Agent.speed = originalSpeed;

        if (movementModifier != null)
        {
            movementModifier.ApplySpeedModifier(1 - chicken.SlowAmount);
            yield return new WaitForSeconds(chicken.SlowDuration);
            movementModifier.RemoveSpeedModifier();
        }
        else
        {
            Debug.LogError("[ChickenSensor] Player doesn't implement IMovementModifier! Add this interface to your player movement script.");
        }

        if (soundEffect != null)
        {
            yield return new WaitUntil(() => !soundEffect.isPlaying);
            soundEffect.Stop();
        }

        if (effect.isPlaying)
        {
            effect.Stop();
        }

        effect.gameObject.SetActive(false);

        yield return new WaitForSeconds(countdown);
        hasSlowed = false;
        isPerformingAction = false;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            Gizmos.color = Color.yellow;
        else
            Gizmos.color = isPlayerDetected ? Color.red : Color.green;

        int segments = 30;
        float step = angle / segments;
        Quaternion baseRotation = Quaternion.LookRotation(transform.forward);

        Vector3 origin = transform.position;
        Vector3 prevPoint = origin + baseRotation * Quaternion.Euler(0, -angle / 2f, 0) * Vector3.forward * range;

        for (int i = 1; i <= segments; i++)
        {
            float yaw = -angle / 2f + step * i;
            Vector3 nextPoint = origin + baseRotation * Quaternion.Euler(0, yaw, 0) * Vector3.forward * range;
            Gizmos.DrawLine(origin, nextPoint);
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }

        prevPoint = origin + baseRotation * Quaternion.Euler(0, -angle / 2f, 0) * Vector3.forward * range;
        for (int i = 1; i <= segments; i++)
        {
            float yaw = -angle / 2f + step * i;
            Vector3 nextPoint = origin + baseRotation * Quaternion.Euler(0, yaw, 0) * Vector3.forward * range;
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }

}
