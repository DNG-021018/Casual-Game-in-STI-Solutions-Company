using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class EggSpawner : MonoBehaviour
{
    public Egg eggPrefab;
    public List<Transform> spawnPoints;
    public Queue<Egg> eggPool = new Queue<Egg>();
    public event Action OnEggPickupScore;
    public event Action<Egg> OnEggTargetChange;
    private Coroutine checkMissingCoroutine;
    private void Start()
    {
        
        for (int i = 0; i < 5; i++)
        {
            Egg egg = Instantiate(eggPrefab, new Vector3(50,50,50), Quaternion.identity, transform);
            egg.gameObject.SetActive(false);
            egg.OnPickedUp += OnPickUpEgg;
            eggPool.Enqueue(egg);
        }

        StartCoroutine(SpawnNewOnPickup());
        checkMissingCoroutine = StartCoroutine(CheckMissingEgg());
    }

    public void OnPickUpEgg(Egg egg)
    {
        ReturnEggToPool(egg);
        OnEggPickupScore?.Invoke();
        StartCoroutine(SpawnNewOnPickup());
    }
    private void ReturnEggToPool(Egg egg)
    {
        eggPool.Enqueue(egg);
    }

    public IEnumerator SpawnNewOnPickup()
    {
        yield return new WaitForSeconds(1f);
        Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];
        if (eggPool.Count > 0)
        {
            Egg egg = eggPool.Dequeue();
            egg.transform.position = spawnPoint.position;
            egg.transform.rotation = spawnPoint.rotation;
            egg.gameObject.SetActive(true);
            OnEggTargetChange?.Invoke(egg);
        }
        else
        {
            Debug.LogWarning("No eggs available in the pool to spawn.");
        }
    }

    IEnumerator CheckMissingEgg()
    {
        yield return new WaitForSeconds(1f);
        if(eggPool.Count == 5)
        {
            yield return new WaitForSeconds(0.1f);
            if(eggPool.Count == 5)
            {
                StartCoroutine(SpawnNewOnPickup());
            }
        }
    }

    private void OnDestroy()
    {
        OnEggPickupScore = null;
        OnEggTargetChange = null;
        StopCoroutine(checkMissingCoroutine);
    }
}
