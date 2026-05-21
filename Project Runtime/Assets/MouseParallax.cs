using UnityEngine;
using System.Collections.Generic;

public class MouseParallax : MonoBehaviour
{
    [Header("Camera (World Space Only)")]
    [SerializeField] private Camera targetCamera;

    [Header("Movement Settings")]
    [SerializeField] private float maxMoveDistance = 0.5f;   // How far the layers can move in units
    [SerializeField] private float smoothSpeed = 5f;        // Lower = smoother/lag, higher = snappy
    [SerializeField] private bool invertX = false;
    [SerializeField] private bool invertY = false;

    [Header("Layers")]
    [SerializeField] private List<ParallaxLayer> layers = new List<ParallaxLayer>();

    private Vector3[] startPositions;

    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform background;
        [Range(-3f, 3f)] public float depthFactor = 1f; // Negative = opposite direction, 0 = static
    }

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        // Save initial positions
        startPositions = new Vector3[layers.Count];
        for (int i = 0; i < layers.Count; i++)
        {
            if (layers[i].background != null)
                startPositions[i] = layers[i].background.position;
        }
    }

    private void Update()
    {
        if (targetCamera == null) return;

        // Get mouse position in viewport (0 to 1)
        Vector3 mouseViewport = targetCamera.ScreenToViewportPoint(Input.mousePosition);
        mouseViewport.x = Mathf.Clamp01(mouseViewport.x);
        mouseViewport.y = Mathf.Clamp01(mouseViewport.y);

        // Convert to -1 to 1 range (centered)
        float offsetX = (mouseViewport.x - 0.5f) * 2f;
        float offsetY = (mouseViewport.y - 0.5f) * 2f;

        if (invertX) offsetX *= -1f;
        if (invertY) offsetY *= -1f;

        // Apply to each layer
        for (int i = 0; i < layers.Count; i++)
        {
            if (layers[i].background == null) continue;

            // Target position
            Vector3 targetPos = startPositions[i];
            targetPos.x += offsetX * maxMoveDistance * layers[i].depthFactor;
            targetPos.y += offsetY * maxMoveDistance * layers[i].depthFactor;

            // Smooth movement
            layers[i].background.position = Vector3.Lerp(
                layers[i].background.position,
                targetPos,
                Time.deltaTime * smoothSpeed
            );
        }
    }

    public void ResetPositions()
    {
        if (startPositions == null) return;
        for (int i = 0; i < layers.Count; i++)
        {
            if (layers[i].background != null && i < startPositions.Length)
                layers[i].background.position = startPositions[i];
        }
    }

    // Visualize range in editor
    private void OnDrawGizmosSelected()
    {
        if (layers == null || startPositions == null) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < layers.Count; i++)
        {
            if (layers[i].background == null) continue;
            Vector3 pos = Application.isPlaying ? startPositions[i] : layers[i].background.position;
            Gizmos.DrawWireCube(pos, new Vector3(maxMoveDistance * 2, maxMoveDistance * 2, 0.1f));
        }
    }
}