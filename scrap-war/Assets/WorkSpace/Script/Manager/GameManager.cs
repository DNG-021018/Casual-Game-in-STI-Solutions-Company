using UnityEngine;
using System;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<float> OnZoneCountdownStarted;

    [SerializeField] private BlueZone blueZone;
    [SerializeField] private float shrinkPercentPerPhase = 0.9f;
    [SerializeField] private float shrinkDuration = 3f;
    [SerializeField] private float intervalBetweenShrink = 10f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(HandleShrinkLoop());
    }

    private IEnumerator HandleShrinkLoop()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            OnZoneCountdownStarted?.Invoke(intervalBetweenShrink);

            yield return new WaitForSeconds(intervalBetweenShrink);

            blueZone.ShrinkOnce(shrinkPercentPerPhase, shrinkDuration);
            yield return new WaitForSeconds(shrinkDuration);
        }
    }
}
