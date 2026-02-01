using UnityEngine;

public class question1 : MonoBehaviour
{
    public Rigidbody2D questionRb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("question1: No SpriteRenderer found on " + gameObject.name);
        }

        // for invisibility at start
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
