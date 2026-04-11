csharp Assets/AreaMusicTrigger.cs
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AreaMusicTrigger : MonoBehaviour
{
    [Tooltip("Audio clip that should play while player is inside this trigger.")]
    public AudioClip areaMusic;

    [Tooltip("Crossfade duration in seconds.")]
    public float crossfade = 1f;

    [Tooltip("If true, music stops (or reverts) when player leaves.")]
    public bool stopOnExit = false;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (areaMusic == null) return;

        if (MusicManager.Instance != null)
            MusicManager.Instance.PlayMusic(areaMusic, crossfade);
        else
            Debug.LogWarning("No MusicManager found in scene. Add MusicManager GameObject with MusicManager component.");
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (!stopOnExit) return;

        if (MusicManager.Instance != null)
            MusicManager.Instance.StopMusic(crossfade);
    }
}