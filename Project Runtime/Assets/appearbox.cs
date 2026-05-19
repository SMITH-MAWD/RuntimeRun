using UnityEngine;

public class appearbox : MonoBehaviour
{
    public BoxCollider2D boxCollider;
    public GameObject OptionBox;

    private PlayerMovement cachedPlayer;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();

        if (boxCollider == null)
        {
            Debug.LogWarning("appearbox: No BoxCollider2D found on " + gameObject.name + ". Add a BoxCollider2D component with 'Is Trigger' enabled.");
        }
        else
        {
            boxCollider.isTrigger = true;
        }

        if (OptionBox != null)
        {
            OptionBox.SetActive(false);
        }
        else
        {
            Debug.LogWarning("appearbox: OptionBox is not assigned. Assign the GameObject in the inspector.");
        }

#if UNITY_2023_2_OR_NEWER
        cachedPlayer = Object.FindFirstObjectByType<PlayerMovement>();
#else
        cachedPlayer = FindObjectOfType<PlayerMovement>();
#endif
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.GetComponent<PlayerMovement>() != null)
        {
            if (OptionBox != null)
            {
                OptionBox.SetActive(true);
            }

            if (cachedPlayer != null)
            {
                cachedPlayer.inputEnabled = false;
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.GetComponent<PlayerMovement>() != null)
        {
            if (OptionBox != null)
            {
                OptionBox.SetActive(false);
            }

            if (cachedPlayer != null)
            {
                cachedPlayer.inputEnabled = true;
            }
        }
    }
}
