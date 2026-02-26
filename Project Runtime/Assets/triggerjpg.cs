using System.Collections;
using UnityEngine;

public class SecretTrigger : MonoBehaviour
{
    [Tooltip("The GameObject (sprite/visual) that will appear for the jumpscare.")]
    public GameObject secretawooo;

    [Tooltip("Audio clip to play when the jumpscare triggers (mp3 or other supported clip).")]
    public AudioClip jumpScareClip;

    [Tooltip("How long (seconds) the jumpscare object stays visible. Keep very small for a flash effect.")]
    public float showDuration = 0.05f;

    // Prevent the jumpscare from triggering repeatedly
    private bool hasTriggered = false;

    // AudioSource used to play the clip. Kept on this object so it continues playing even if the visual is deactivated.
    private AudioSource audioSource;

    void Awake()
    {
        // Ensure we have an AudioSource on this GameObject
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            StartCoroutine(DoJumpScare());
        }
    }

    private IEnumerator DoJumpScare()
    {
        if (secretawooo != null)
            secretawooo.SetActive(true);

        if (jumpScareClip != null && audioSource != null)
            audioSource.PlayOneShot(jumpScareClip);

        // Wait the configured short time so the player sees the flash. If you truly want "instant", set this to 0.
        if (showDuration > 0f)
            yield return new WaitForSeconds(showDuration);

        if (secretawooo != null)
            secretawooo.SetActive(false);

        // Optional: disable this trigger so it doesn't run again. Comment out if you want repeatable triggers.
        // gameObject.SetActive(false);
    }
}