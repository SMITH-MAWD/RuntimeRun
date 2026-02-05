using UnityEngine;

public class smallplatscrpt7 : MonoBehaviour
{
    [Tooltip("Alpha value to apply to the platform's SpriteRenderer (0 = fully transparent, 1 = opaque)")]
    public float alpha = 0.5f;

    private SpriteRenderer sr;
    private float originalAlpha = 1f;
    private bool collidersDisabled = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {

            originalAlpha = sr.color.a;
            Color c = sr.color;
            c.a = Mathf.Clamp01(alpha);
            sr.color = c;
        }
        else
        {
            Debug.LogWarning("smallplatscrpt: No SpriteRenderer found on " + gameObject.name + ". Attach this script to objects with SpriteRenderer.");
        }

        // Disable Collider2D so that the player will just fall through the platform
        Collider2D[] cols = GetComponents<Collider2D>();
        if (cols != null && cols.Length > 0)
        {
            foreach (var col in cols)
            {
                if (col.enabled)
                    col.enabled = false;
            }
            collidersDisabled = true;
        }
        /// ignore this this is for debugging really helpful tbh
        else
        {
            Debug.LogWarning("smallplatscrpt: No Collider2D found on " + gameObject.name + ". If you expect a collider, add a Collider2D component.");
        }
    }


    public void Reveal()
    {
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f; // to make fully visible
            sr.color = c;
        }

        Collider2D[] cols = GetComponents<Collider2D>();
        if (cols != null && cols.Length > 0)
        {
            foreach (var col in cols)
            {
                col.enabled = true;
            }
            collidersDisabled = false;
        }
        else
        {
            Debug.LogWarning("smallplatscrpt.Reveal: No Collider2D found on " + gameObject.name + ". Cannot enable colliders.");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
