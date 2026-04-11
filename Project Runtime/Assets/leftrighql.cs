using UnityEngine;

public class leftrighql : MonoBehaviour
{
    [Tooltip("Half distance (in units) the object will move left and right from its start position")]
    public float amplitude = 0.25f;

    [Tooltip("Oscillation speed in cycles per second")]
    public float frequency = 1f;

    [Tooltip("Use localPosition (true) or world position (false)")]
    public bool useLocalPosition = true;

    // cached start position
    private Vector3 startPos;

    void Start()
    {
        startPos = useLocalPosition ? transform.localPosition : transform.position;
    }

    void Update()
    {
        // sine-based oscillation on X axis
        float x = Mathf.Sin(Time.time * Mathf.PI * 2f * frequency) * Mathf.Abs(amplitude);
        Vector3 offset = new Vector3(x, 0f, 0f);

        if (useLocalPosition)
            transform.localPosition = startPos + offset;
        else
            transform.position = startPos + offset;
    }
}
