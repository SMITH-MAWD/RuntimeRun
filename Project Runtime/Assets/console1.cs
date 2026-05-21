using UnityEngine;
using System.Collections;

public class console1 : MonoBehaviour
{
    [Tooltip("Reference to the question box GameObject")]
    public GameObject questionBox;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.4f;        // How long the fade takes
    [SerializeField] private CanvasGroup questionCanvasGroup;  // Optional – auto-added if missing

    private bool isQuestionVisible = false;
    private bool isFading = false;
    private Coroutine currentFadeRoutine;

    // Track the most recently used console for respawn functionality
    private static console1 mostRecentConsole;
    public static console1 GetMostRecentConsole() => mostRecentConsole;

    public BoxCollider2D boxCollider;
    private PlayerMovement cachedPlayer;
    private bool isPlayerInRange = false;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            Debug.LogWarning("Console1: No BoxCollider2D found on " + gameObject.name + ". Add a BoxCollider2D component with 'Is Trigger' enabled.");
        }
        else
        {
            boxCollider.isTrigger = true;
        }

        if (questionBox == null)
        {
            questionBox = GameObject.Find("question1");
            if (questionBox == null)
            {
                Debug.LogWarning("Console1: Question Box not assigned. Assign the question box GameObject in the inspector.");
            }
        }

        if (questionBox != null)
        {
            // Ensure a CanvasGroup exists for fading
            if (questionCanvasGroup == null)
                questionCanvasGroup = questionBox.GetComponent<CanvasGroup>();

            if (questionCanvasGroup == null)
                questionCanvasGroup = questionBox.AddComponent<CanvasGroup>();

            // Start fully hidden (invisible and inactive)
            questionCanvasGroup.alpha = 0f;
            questionBox.SetActive(false);
            isQuestionVisible = false;
        }

        // Cache the player to avoid Find() calls
#if UNITY_2023_2_OR_NEWER
        cachedPlayer = Object.FindFirstObjectByType<PlayerMovement>();
#else
        cachedPlayer = FindObjectOfType<PlayerMovement>();
#endif
    }

    void Update()
    {
        // Check if player is in range and presses Q
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.Q))
        {
            OnConsoleInteract();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.GetComponent<PlayerMovement>() != null)
        {
            isPlayerInRange = true;
            Debug.Log("Console1: Player in range. Press Q to open console.");
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.GetComponent<PlayerMovement>() != null)
        {
            isPlayerInRange = false;

            // If the question is visible, close it with a fade-out
            if (isQuestionVisible && questionBox != null)
            {
                CloseQuestion();
            }

            Debug.Log("Console1: Player out of range.");
        }
    }

    void OnConsoleInteract()
    {
        // Mark this console as the most recently used
        mostRecentConsole = this;

        if (questionBox == null) return;

        // Toggle visibility
        if (!isQuestionVisible)
        {
            OpenQuestion();
        }
        else
        {
            CloseQuestion();
        }
    }

    /// <summary>Opens the question box with a fade-in, and disables player movement.</summary>
    private void OpenQuestion()
    {
        if (isQuestionVisible) return;

        isQuestionVisible = true;

        // Disable player input immediately
        if (cachedPlayer != null)
            cachedPlayer.inputEnabled = false;

        // Start fade-in
        StartFade(1f);
    }

    /// <summary>Closes the question box with a fade-out, and re‑enables player movement.</summary>
    private void CloseQuestion()
    {
        if (!isQuestionVisible) return;

        isQuestionVisible = false;

        // Re‑enable player input immediately (so they can move while fading, if desired)
        if (cachedPlayer != null)
            cachedPlayer.inputEnabled = true;

        // Start fade-out
        StartFade(0f);
    }

    /// <summary>Starts a fade to the target alpha (0 or 1).</summary>
    private void StartFade(float targetAlpha)
    {
        if (questionCanvasGroup == null) return;

        // Stop any ongoing fade
        if (currentFadeRoutine != null)
            StopCoroutine(currentFadeRoutine);

        // Ensure the GameObject is active when fading in
        if (targetAlpha > 0f && !questionBox.activeSelf)
            questionBox.SetActive(true);

        currentFadeRoutine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        isFading = true;
        float startAlpha = questionCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            questionCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        // Ensure exact final value
        questionCanvasGroup.alpha = targetAlpha;

        // Deactivate GameObject when completely invisible (to disable raycasts, etc.)
        if (targetAlpha == 0f)
        {
            questionBox.SetActive(false);
            // Reset alpha to 0 so it stays consistent next time it's activated
            questionCanvasGroup.alpha = 0f;
        }

        isFading = false;
        currentFadeRoutine = null;
    }
}