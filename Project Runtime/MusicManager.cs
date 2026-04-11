csharp Assets/MusicManager.cs
using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] float defaultCrossfade = 1f;

    AudioSource _a;
    AudioSource _b;
    AudioSource _active;
    AudioSource _inactive;
    Coroutine _crossfadeCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _a = gameObject.AddComponent<AudioSource>();
        _b = gameObject.AddComponent<AudioSource>();

        _a.playOnAwake = false;
        _b.playOnAwake = false;
        _a.loop = true;
        _b.loop = true;

        _active = _a;
        _inactive = _b;
    }

    public void PlayMusic(AudioClip clip, float crossfadeDuration = -1f)
    {
        if (clip == null) return;
        if (crossfadeDuration < 0f) crossfadeDuration = defaultCrossfade;
        if (_active.isPlaying && _active.clip == clip) return;

        if (_crossfadeCoroutine != null) StopCoroutine(_crossfadeCoroutine);
        _crossfadeCoroutine = StartCoroutine(CrossfadeTo(clip, crossfadeDuration));
    }

    public void StopMusic(float fadeOutDuration = 0.5f)
    {
        if (_crossfadeCoroutine != null) StopCoroutine(_crossfadeCoroutine);
        _crossfadeCoroutine = StartCoroutine(FadeOutAndStop(fadeOutDuration));
    }

    IEnumerator CrossfadeTo(AudioClip clip, float duration)
    {
        // prepare inactive source
        _inactive.clip = clip;
        _inactive.volume = 0f;
        _inactive.Play();

        float t = 0f;
        float startVolActive = _active.volume;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            _inactive.volume = Mathf.Lerp(0f, 1f, p);
            _active.volume = Mathf.Lerp(startVolActive, 0f, p);
            yield return null;
        }

        // finish
        _inactive.volume = 1f;
        _active.volume = 0f;
        _active.Stop();

        // swap
        var tmp = _active;
        _active = _inactive;
        _inactive = tmp;
        _crossfadeCoroutine = null;
    }

    IEnumerator FadeOutAndStop(float duration)
    {
        float startVol = _active.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            _active.volume = Mathf.Lerp(startVol, 0f, t / duration);
            yield return null;
        }
        _active.Stop();
        _active.volume = startVol;
        _crossfadeCoroutine = null;
    }
}