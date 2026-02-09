using UnityEngine;

// NOTE: In Unity, a MonoBehaviour script must have a class name that matches the file name
// (so this component can be found/loaded when referenced from a scene).
public class console1 : MonoBehaviour
{
    [Tooltip("Reference to the question box GameObject")]
    public GameObject questionBox;
    private bool isQuestionVisible = false;

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
            // Ensure the collider is set as a trigger
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
            // hiding the question box
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
        // Check if the colliding object has a PlayerMovement component
        if (collision.CompareTag("Player") || collision.GetComponent<PlayerMovement>() != null)
        {
            isPlayerInRange = true;
            Debug.Log("Console1: Player in range. Press Q to open console.");
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        // Check if the colliding object is the player
        if (collision.CompareTag("Player") || collision.GetComponent<PlayerMovement>() != null)
        {
            isPlayerInRange = false;

            // Close the console if player leaves range
            if (isQuestionVisible)
            {
                isQuestionVisible = false;
                if (questionBox != null)
                {
                    questionBox.SetActive(false);
                    Debug.Log("Question box hidden as player left range.");
                }

                // Re-enable player movement
                if (cachedPlayer != null)
                {
                    cachedPlayer.inputEnabled = true;
                }
            }

            Debug.Log("Console1: Player out of range.");
        }
    }

    void OnConsoleInteract()
    {
        if (questionBox != null)
        {
            // visibility toggle
            isQuestionVisible = !isQuestionVisible;
            questionBox.SetActive(isQuestionVisible);
            Debug.Log("Question box visibility toggled: " + (isQuestionVisible ? "visible" : "hidden"));
        }

        // enable or disable the player movement when question box is open
        if (cachedPlayer != null)
        {
            // when question is visible = no movement
            cachedPlayer.inputEnabled = !isQuestionVisible;
        }
    }
}
