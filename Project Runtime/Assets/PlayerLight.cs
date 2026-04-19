using UnityEngine;
using UnityEngine.Rendering.Universal;

[DisallowMultipleComponent]
public class PlayerLight : MonoBehaviour
{
    [Header("Assign an existing Light2D from the Hierarchy (preferred)")]
    [Tooltip("If assigned, this Light2D will be used instead of creating a new one.")]
    [SerializeField] private Light2D lightFromHierarchy;

    [Header("Runtime creation (used only if no hierarchy light assigned)")]
    [Tooltip("If true and no hierarchy Light2D is assigned/found, the script will create one at runtime.")]
    [SerializeField] private bool createIfMissing = false;

    [Header("Light properties")]
    public Color color = Color.white;
    [Range(0f, 10f)] public float intensity = 1f;
    [Tooltip("Inner radius for point/spot lights")]
    public float innerRadius = 0.5f;
    [Tooltip("Outer radius for point/spot lights")]
    public float outerRadius = 3f;

    [Header("Spot (cone) settings")]
    [Tooltip("Enable to use a cone-shaped (spot) 2D light")]
    public bool useSpot = false;
    [Range(0f, 360f)] public float spotInnerAngle = 20f;
    [Range(0f, 360f)] public float spotOuterAngle = 40f;
    [Tooltip("Local Z rotation applied to the Light GameObject to aim the cone (degrees)")]
    public float spotLocalZRotation = 0f;

    [Header("Follow / offset")]
    [Tooltip("Keep the Light2D positioned at player + this offset")]
    public Vector3 offset = Vector3.zero;
    [Tooltip("If true the light GameObject will be parented to the player transform")]
    public bool parentToPlayer = true;

    private Light2D _activeLight;

    void Awake()
    {
        // Prefer the explicitly assigned light from the Hierarchy.
        if (lightFromHierarchy != null)
        {
            _activeLight = lightFromHierarchy;
        }
        else
        {
            // Try to find a Light2D on children (maybe you created it under the player)
            _activeLight = GetComponentInChildren<Light2D>(includeInactive: true);
        }

        // Create one if allowed and none found
        if (_activeLight == null && createIfMissing)
        {
            var go = new GameObject("Player Light2D (auto)");
            if (parentToPlayer)
                go.transform.SetParent(transform, worldPositionStays: false);
            else
                go.transform.position = transform.position + offset;
            go.transform.localPosition = Vector3.zero;
            _activeLight = go.AddComponent<Light2D>();
        }

        if (_activeLight == null)
        {
            Debug.LogWarning($"PlayerLight: No Light2D assigned or found for '{gameObject.name}'. Assign one in the inspector or enable CreateIfMissing.");
            return;
        }

        // Configure the light using serialized properties
        ApplyPropertiesToLight();
    }

    void LateUpdate()
    {
        if (_activeLight == null) return;

        // Keep the light positioned on the player with the chosen offset
        if (parentToPlayer)
        {
            // If parented, ensure localPosition is offset (useful if created at runtime)
            _activeLight.transform.localPosition = offset;
        }
        else
        {
            _activeLight.transform.position = transform.position + offset;
        }

        // If using spot mode, ensure the cone is aimed by local Z rotation
        if (useSpot)
        {
            _activeLight.transform.localEulerAngles = new Vector3(0f, 0f, spotLocalZRotation);
        }
    }

    // Apply changes immediately (useful in editor / after changing inspector values)
    public void ApplyPropertiesToLight()
    {
        if (_activeLight == null) return;

        _activeLight.lightType = Light2D.LightType.Point;
        _activeLight.color = color;
        _activeLight.intensity = intensity;
        _activeLight.pointLightInnerRadius = innerRadius;
        _activeLight.pointLightOuterRadius = outerRadius;

        if (useSpot)
        {
            _activeLight.pointLightInnerAngle = Mathf.Clamp(spotInnerAngle, 0f, 360f);
            _activeLight.pointLightOuterAngle = Mathf.Clamp(spotOuterAngle, 0f, 360f);
        }
        else
        {
            _activeLight.pointLightInnerAngle = 360f;
            _activeLight.pointLightOuterAngle = 360f;
        }

        _activeLight.enabled = true;
    }

    // Editor helper - reflect inspector changes at runtime without re-entering Play mode
    void OnValidate()
    {
        // If the user assigned a light in the inspector after Awake, use it.
        if (Application.isPlaying)
        {
            if (lightFromHierarchy != null && _activeLight != lightFromHierarchy)
            {
                _activeLight = lightFromHierarchy;
                ApplyPropertiesToLight();
            }
            else
            {
                ApplyPropertiesToLight();
            }
        }
    }
}
