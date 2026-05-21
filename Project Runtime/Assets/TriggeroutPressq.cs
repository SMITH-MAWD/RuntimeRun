using UnityEngine;

public class TriggeroutPressq : MonoBehaviour
{
    [Header("Target Text")]
    public FadeInText textToFade;           // Drag the GameObject with FadeInText here

    [Header("Settings")]
    public bool requireKeyPress = true;     // true = press Q to disappear
    public KeyCode key = KeyCode.Q;

    private bool playerInRange = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            // If no key is required, the text disappears instantly on entering the trigger
            if (!requireKeyPress)
                TriggerFadeOut();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    void Update()
    {
        // When a key press is required, check every frame
        if (requireKeyPress && playerInRange && Input.GetKeyDown(key))
        {
            TriggerFadeOut();
        }
    }

    private void TriggerFadeOut()
    {
        if (textToFade != null)
        {
            textToFade.FadeOut();
        }
        else
        {
            Debug.LogWarning("TriggeroutPressq: No FadeInText assigned in the inspector.");
        }
    }
}