using System;
using System.Collections;
using UnityEngine;

public class Egg : MonoBehaviour, IPickUpObject
{
    [SerializeField] private ParticleSystem _pickUpEffect;
    [SerializeField] private ParticleSystem _showEffect;
    [SerializeField] private GameObject _eggVisual;
    [SerializeField] private SphereCollider _collider;
    [SerializeField] private AudioClip _pickUpSound;
    public event Action<Egg> OnPickedUp;
    private void Awake()
    {
        
    }

    private void OnEnable()
    {
        OnSpawned();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnPickUp();
            Debug.Log("Egg picked up by player!");
        }
    }

    public IEnumerator OnPickUpEffect()
    {
        OnPickedUp?.Invoke(this);
        _collider.enabled = false; // Disable the collider to prevent further pickups
        yield return new WaitForSeconds(0.2f); // Delay to allow for visual feedback
        _pickUpEffect.Play();
        yield return new WaitForSeconds(_pickUpEffect.main.duration/5f);
        // play pick up sound
        if (_pickUpSound != null)
        {
            SoundManager.Instance.PlaySFX(_pickUpSound);
        }
        _eggVisual.SetActive(false);
        _showEffect.Stop();
        gameObject.SetActive(false);
    }

    public void OnPickUp()
    {
        StartCoroutine(OnPickUpEffect());
    }

    public void OnSpawned()
    {
        if (_showEffect != null)
        {
            _showEffect.Play();
        }
        if (_eggVisual != null)
        {
            _eggVisual.SetActive(true);
        }
        if (_collider != null)
        {
            _collider.enabled = true;
        }
    }

    private void OnDestroy()
    {
        OnPickedUp = null;
        if (_pickUpEffect != null)
        {
            _pickUpEffect.Stop();
        }
        if (_showEffect != null)
        {
            _showEffect.Stop();
        }
    }
}
