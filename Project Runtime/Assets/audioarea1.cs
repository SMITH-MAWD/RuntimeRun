using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class audioarea1 : MonoBehaviour
{
    [Header("Audio Area Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool playOnce = false;
    [SerializeField] private bool stopOnExit = true;
    [SerializeField] private AudioSource audioSource; // optional: assign in Inspector or it will GetComponent

    private bool hasPlayed;

    void Awake()
    {
        // Prefer Awake so we can disable playOnAwake before Unity may play the clip.
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogWarning($"AudioSource not found on '{gameObject.name}'.");
            return;
        }

        // Ensure the AudioSource won't automatically start on scene load.
        audioSource.playOnAwake = false;
        // Make sure no clip is playing from a previous state.
        if (audioSource.isPlaying)
            audioSource.Stop();

        hasPlayed = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (audioSource == null) return;
        if (!other.CompareTag(playerTag)) return;

        if (playOnce)
        {
            if (hasPlayed) return;
            audioSource.Play();
            hasPlayed = true;
        }
        else
        {
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
    }                                               

    void OnTriggerExit2D(Collider2D other)
    {
        if (audioSource == null) return;
        if (!other.CompareTag(playerTag)) return;

        if (stopOnExit && audioSource.isPlaying)
            audioSource.Stop();
    }
}
