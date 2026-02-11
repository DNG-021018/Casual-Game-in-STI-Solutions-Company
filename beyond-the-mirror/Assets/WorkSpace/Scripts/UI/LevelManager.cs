using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Components")]
    private SegmentManager segmentManager;
    private CharacterController[] characterController;

    public static LevelManager Instance { get; private set; }

    public event System.Action<bool> OnEndGame;

    private void OnEnable()
    {
        CharacterController.OnAnyPlayerHit += HandleAnyPlayerHit;
        CharacterController.OnAnySkillActivated += HandleAnySkillActivated;
    }

    private void OnDisable()
    {
        CharacterController.OnAnyPlayerHit -= HandleAnyPlayerHit;
        CharacterController.OnAnySkillActivated -= HandleAnySkillActivated;
    }

    public void InitializeLevel()
    {
        segmentManager = FindAnyObjectByType<SegmentManager>();
        characterController = FindObjectsByType<CharacterController>(FindObjectsSortMode.None);
    }

    private void HandleAnyPlayerHit()
    {
        // Debug.Log("Any player hit → all die");

        foreach (var controller in characterController)
        {
            controller.ForceKill();
        }

        segmentManager.StopAllSegments();
        StartCoroutine(InvokeEndGameAfterDelay(3f));
    }

    private void HandleAnySkillActivated()
    {
        // Debug.Log("Any skill activated → all active");

        foreach (var controller in characterController)
        {
            controller.ForceActiveSkill();
        }
    }

    private IEnumerator InvokeEndGameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnEndGame?.Invoke(false);
    }
}