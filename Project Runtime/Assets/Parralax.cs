using UnityEngine;
using System.Collections.Generic;

public class Parallax : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Parallax Settings")]
    [SerializeField] private Vector2 moveDirection = Vector2.left;

    [Header("Infinite Scroll")]
    [SerializeField] private bool enableInfiniteScroll = true;
    [SerializeField] private bool horizontalWrap = true;
    [SerializeField] private bool verticalWrap = false;

    [Header("Background Layers (Drag & Set Speed)")]
    [SerializeField] private List<ParallaxLayer> layers = new List<ParallaxLayer>();

    private Vector3 lastCameraPosition;

    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform background;
        [Range(0f, 3f)] public float speed = 0.5f;
        [HideInInspector] public Vector3 startPosition;
        [HideInInspector] public float spriteWidth;
        [HideInInspector] public float spriteHeight;
    }

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        lastCameraPosition = targetCamera.transform.position;

        foreach (var layer in layers)
        {
            if (layer.background == null) continue;

            layer.startPosition = layer.background.position;

            // Calculate sprite size for wrapping
            SpriteRenderer sr = layer.background.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                layer.spriteWidth = sr.sprite.bounds.size.x * layer.background.localScale.x;
                layer.spriteHeight = sr.sprite.bounds.size.y * layer.background.localScale.y;
            }
            else
            {
                // Fallback: use local scale or BoxCollider2D
                layer.spriteWidth = layer.background.localScale.x;
                layer.spriteHeight = layer.background.localScale.y;
            }
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        Vector3 delta = targetCamera.transform.position - lastCameraPosition;
        delta = new Vector3(delta.x * moveDirection.x, delta.y * moveDirection.y, 0);

        foreach (var layer in layers)
        {
            if (layer.background == null) continue;

            Vector3 move = new Vector3(delta.x * layer.speed, delta.y * layer.speed, 0);
            layer.background.position += move;

            // Infinite scroll wrapping
            if (enableInfiniteScroll)
            {
                if (horizontalWrap && layer.spriteWidth > 0)
                {
                    float offsetX = layer.background.position.x - layer.startPosition.x;
                    if (Mathf.Abs(offsetX) >= layer.spriteWidth)
                    {
                        float wrappedOffset = Mathf.Round(offsetX / layer.spriteWidth) * layer.spriteWidth;
                        layer.background.position = new Vector3(
                            layer.startPosition.x + wrappedOffset,
                            layer.background.position.y,
                            layer.background.position.z
                        );
                    }
                }

                if (verticalWrap && layer.spriteHeight > 0)
                {
                    float offsetY = layer.background.position.y - layer.startPosition.y;
                    if (Mathf.Abs(offsetY) >= layer.spriteHeight)
                    {
                        float wrappedOffset = Mathf.Round(offsetY / layer.spriteHeight) * layer.spriteHeight;
                        layer.background.position = new Vector3(
                            layer.background.position.x,
                            layer.startPosition.y + wrappedOffset,
                            layer.background.position.z
                        );
                    }
                }
            }
        }

        lastCameraPosition = targetCamera.transform.position;
    }

    public void ResetPositions()
    {
        foreach (var layer in layers)
        {
            if (layer.background != null)
                layer.background.position = layer.startPosition;
        }
        lastCameraPosition = targetCamera.transform.position;
    }
}