using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Linq;
using System.Reflection;

public class BuildDiagnostics : MonoBehaviour
{
    void Start()
    {
        // --- ShadowCaster2D ---
        var casters = FindObjectsByType<ShadowCaster2D>(FindObjectsSortMode.None);
        Debug.Log($"[BuildDiagnostics] Found {casters.Length} ShadowCaster2D(s) in scene.");

        foreach (var c in casters)
        {
            string sortingLayersInfo = "?";
            try
            {
                // Access private field m_ApplyToSortingLayers via reflection
                var field = typeof(ShadowCaster2D).GetField("m_ApplyToSortingLayers", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    var val = field.GetValue(c);
                    if (val is int[] arr && arr.Length > 0)
                        sortingLayersInfo = string.Join(",", arr);
                    else
                        sortingLayersInfo = "ALL (null/empty)";
                }
            }
            catch { sortingLayersInfo = "error"; }

            Debug.Log($" - {c.name} | Active: {c.gameObject.activeInHierarchy} | " +
                      $"Layer: {LayerMask.LayerToName(c.gameObject.layer)} | " +
                      $"SelfShadows: {c.selfShadows} | " +
                      $"SortingLayers: {sortingLayersInfo}");
        }

        // --- Global Light2D ---
        var globalLights = FindObjectsByType<Light2D>(FindObjectsSortMode.None)
                            .Where(l => l.lightType == Light2D.LightType.Global)
                            .ToArray();

        Debug.Log($"[BuildDiagnostics] Found {globalLights.Length} Global Light2D(s):");
        foreach (var l in globalLights)
        {
            int cullingMaskValue = -1;
            try
            {
                // Access private field m_CullingMask (or cullingMask) via reflection
                var field = typeof(Light2D).GetField("m_CullingMask", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                    cullingMaskValue = (int)field.GetValue(l);
            }
            catch { }

            Debug.Log($" - {l.name} | Enabled: {l.enabled} | ActiveGO: {l.gameObject.activeInHierarchy} | " +
                      $"Intensity: {l.intensity} | CullingMask: {(cullingMaskValue >= 0 ? cullingMaskValue.ToString() : "?")}");
        }

        // --- Quality & Pipeline ---
        Debug.Log($"[BuildDiagnostics] Quality Level: {QualitySettings.names[QualitySettings.GetQualityLevel()]}");
        Debug.Log($"[BuildDiagnostics] Render Pipeline: {UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline?.name ?? "None"}");
    }
}