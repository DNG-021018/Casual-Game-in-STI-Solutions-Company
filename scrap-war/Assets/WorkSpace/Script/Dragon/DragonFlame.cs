using System.Collections;
using UnityEngine;

public class DragonFlame : MonoBehaviour
{
    private ParticleSystem flameEffect;
    private float initialRateOverTime;
    private Transform pointLight;

    void Start()
    {
        flameEffect = GetComponent<ParticleSystem>();
        ValidationUtils.CheckNull(flameEffect, "[DragonFlame.cs] ---> Flame Effect is null");

        pointLight = GetComponentInChildren<Transform>();
        ValidationUtils.CheckNull(pointLight, "[DragonFlame.cs] ---> Point Light is null");

        ParticleSystem.EmissionModule emission = flameEffect.emission;
        initialRateOverTime = emission.rateOverTime.constant;

        flameEffect.gameObject.SetActive(false);
        pointLight.gameObject.SetActive(false);
    }

    public void PlayFlame()
    {
        flameEffect.gameObject.SetActive(true);
        pointLight.gameObject.SetActive(true);

        flameEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        flameEffect.Clear(true);

        ParticleSystem.EmissionModule emission = flameEffect.emission;
        ParticleSystem.MinMaxCurve rateOverTime = emission.rateOverTime;
        rateOverTime.constant = initialRateOverTime;
        emission.rateOverTime = rateOverTime;

        flameEffect.Play(true);
    }

    public void StopFlame(float duration = 1f)
    {
        StartCoroutine(SmoothStop(duration));
    }

    private IEnumerator SmoothStop(float duration)
    {
        ParticleSystem.EmissionModule emission = flameEffect.emission;
        float startRate = emission.rateOverTime.constant;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float currentRate = Mathf.Lerp(startRate, 0f, t);
            ParticleSystem.MinMaxCurve rateOverTime = emission.rateOverTime;
            rateOverTime.constant = currentRate;
            emission.rateOverTime = rateOverTime;

            elapsed += Time.deltaTime;
            yield return null;
        }

        emission.rateOverTime = 0f;
        flameEffect.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        yield return new WaitForSeconds(flameEffect.main.startLifetime.constantMax);
        flameEffect.gameObject.SetActive(false);
        pointLight.gameObject.SetActive(false);
    }
}
