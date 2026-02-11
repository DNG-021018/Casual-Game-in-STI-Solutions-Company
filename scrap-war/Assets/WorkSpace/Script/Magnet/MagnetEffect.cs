using System.Collections;
using UnityEngine;

[System.Serializable]
public class MagnetEffect : MagnetComponent
{
    [SerializeField] ParticleSystem pullEffect;

    public override void Initialize(MagnetController controller)
    {
        base.Initialize(controller);

        ValidationUtils.CheckNull(pullEffect, $"[MagnetController] ---> pullEffect is null");
        ValidationUtils.CheckNull(shootEffect, $"[MagnetController] ---> shootEffect is null");

        pullEffect.gameObject.SetActive(false);
        shootEffect.gameObject.SetActive(false);
    }

    public void StopAllEffects()
    {
        StopPullEffect();
        StopShootEffect();
    }

    public void PlayPullEffect()
    {
        bool hasItem = magnetController.itemsHolder.childCount > 0;

        if (!hasItem && !pullEffect.isPlaying)
        {
            pullEffect.gameObject.SetActive(true);
            pullEffect.Play();
        }
        else if (hasItem && pullEffect.isPlaying)
        {
            StopPullEffect();
        }
    }

    public void StopPullEffect()
    {
        if (pullEffect.isPlaying || pullEffect.gameObject.activeInHierarchy)
        {
            pullEffect.Stop();
            pullEffect.gameObject.SetActive(false);
        }
    }

    [SerializeField] ParticleSystem shootEffect;
    public void PlayShootEffect()
    {
        if (!shootEffect.isPlaying)
        {
            shootEffect.gameObject.SetActive(true);
            shootEffect.Play();
            magnetController.StartCoroutine(StopShootEffectAfterSeconds(shootEffect.main.duration));
        }
    }

    private IEnumerator StopShootEffectAfterSeconds(float delay)
    {
        yield return new WaitForSeconds(delay);
        StopShootEffect();
    }

    public void StopShootEffect()
    {
        if (shootEffect.isPlaying || shootEffect.gameObject.activeInHierarchy)
        {
            shootEffect.Stop();
            shootEffect.gameObject.SetActive(false);
        }
    }


    public override void Update() { }
    public override void OnTriggerExit(Collider other) { }
    public override void OnTriggerStay(Collider other) { }
}
