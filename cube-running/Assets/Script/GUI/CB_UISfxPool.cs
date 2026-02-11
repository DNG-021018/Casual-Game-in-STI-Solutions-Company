using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CB_CubeRunner
{
    public class CR_UISfxPool : MonoBehaviour
    {
        [SerializeField] AudioSource sourcePrefab;
        [SerializeField] int prewarm = 4;

        readonly Queue<AudioSource> _pool = new();

        void Awake()
        {
            for (int i = 0; i < prewarm; i++) _pool.Enqueue(Create());
        }

        AudioSource Create()
        {
            AudioSource s = Instantiate(sourcePrefab, transform);
            s.playOnAwake = false;
            s.enabled = false;
            s.gameObject.SetActive(false);
            return s;
        }

        AudioSource Get()
        {
            if (_pool.Count == 0) _pool.Enqueue(Create());
            return _pool.Dequeue();
        }

        public void PlayOneShot(AudioClip clip, float volume = 1f, Action onDone = null)
        {
            AudioSource s = Get();
            s.clip = clip;
            s.volume = volume;
            s.enabled = true;
            s.gameObject.SetActive(true);
            s.Play();
            onDone?.Invoke();

            StartCoroutine(ReturnWhenDone(s, clip.length / Mathf.Max(0.01f, s.pitch), onDone));
        }

        IEnumerator ReturnWhenDone(AudioSource s, float time, Action onDone)
        {
            yield return new WaitForSecondsRealtime(time);
            s.Stop();
            s.enabled = false;
            s.gameObject.SetActive(false);
            _pool.Enqueue(s);
        }
    }
}
