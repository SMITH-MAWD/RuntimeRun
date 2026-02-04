using UnityEngine;

// NOTE: In Unity, a MonoBehaviour script must have a class name that matches the file name
// (so this component can be found/loaded when referenced from a scene).
public class console1 : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public Color hoverColor = new Color(0.8f, 0.8f, 0.8f, 1f); // hover effect
    private Color originalColor;
    private bool isSetup = false;

    [Tooltip("Reference to the question box GameObject")]
    public GameObject questionBox;
    private bool isQuestionVisible = false;

    public BoxCollider2D boxCollider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        BoxCollider2D triggerCollider = GetComponent<BoxCollider2D>();

        if (spriteRenderer == null)
        {
            Debug.LogWarning("Console1: No SpriteRenderer found on " + gameObject.name + ". Add a SpriteRenderer for visual feedback.");
        }
        else
        {
            originalColor = spriteRenderer.color;
            isSetup = true;
        }

        if (triggerCollider == null)
        {
            Debug.LogWarning("Console1: No BoxCollider2D found on " + gameObject.name + ". Add a BoxCollider2D component for click detection.");
        }

        if (questionBox == null)
        {
            Debug.LogWarning("Console1: Question Box not assigned. Assign the question box GameObject in the inspector.");
        }
        else
        {
            // hiding the question box
            questionBox.SetActive(false);
            isQuestionVisible = false;
        }
    }

    void OnMouseDown()
    {
        // if the question box is missing, try to find by name???
        if (questionBox == null)
        {
            questionBox = GameObject.Find("question1");
            if (questionBox == null)
                Debug.LogWarning("Console1: questionBox not assigned and 'question1' GameObject not found in scene.");
        }

        if (questionBox != null)
        {
            // visibility toggle
            isQuestionVisible = !isQuestionVisible;
            questionBox.SetActive(isQuestionVisible);
            Debug.Log("Question box visibility toggled: " + (isQuestionVisible ? "visible" : "hidden"));
        }
        // enable or disable the player movement when question box is open
        PlayerMovement player = null;
#if UNITY_2023_2_OR_NEWER
        player = Object.FindFirstObjectByType<PlayerMovement>();
#else
        player = FindObjectOfType<PlayerMovement>();
#endif

        if (player != null)
        {
            // when question is visible = no movement
            player.inputEnabled = !isQuestionVisible;
        }
        else
        {
            Debug.LogWarning("Console1: No PlayerMovement found in the scene to toggle input.");
        }
    }

    void OnMouseEnter()
    {
        // Show hover effect
        if (isSetup && spriteRenderer != null)
        {
            spriteRenderer.color = hoverColor;
        }
    }

    void OnMouseExit()
    {
        // Reset color when mouse exits
        if (isSetup && spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
}
