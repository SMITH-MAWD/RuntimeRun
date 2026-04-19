
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

[ExecuteAlways]
[DisallowMultipleComponent]
public class ShadowCaster2DCreator : MonoBehaviour
{
    [Header("Shadow Settings")]
    [SerializeField] private bool selfShadows = false;
    [SerializeField] private string sortingLayerA = "BG";
    [SerializeField] private string sortingLayerB = "Entities";

    [Header("Generation")]
    [SerializeField] private string childPrefix = "__shadow_caster_";
    [SerializeField] private bool markAsEditorOnly = true;

    // Reflection targets
    static FieldInfo fiShapePath, fiShapePathHash, fiApplyToSortingLayers;
    static bool reflectionReady;

    void OnEnable() => EnsureReflection();

    static void EnsureReflection()
    {
        if (reflectionReady) return;

        var scType = typeof(ShadowCaster2D);
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;

        fiShapePath = scType.GetField("m_ShapePath", flags);
        fiShapePathHash = scType.GetField("m_ShapePathHash", flags);
        fiApplyToSortingLayers = scType.GetField("m_ApplyToSortingLayers", flags);

        reflectionReady = (fiShapePath != null && fiShapePathHash != null && fiApplyToSortingLayers != null);
    }

    public void Cleanup()
    {
        var existing = new List<GameObject>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.name.StartsWith(childPrefix) || child.GetComponent<ShadowCaster2D>())
                existing.Add(child.gameObject);
        }
        foreach (var go in existing)
            DestroyImmediate(go);
    }

    public void Create()
    {
        EnsureReflection();
        Cleanup();

        var allPaths = CollectPaths();
        if (allPaths.Count == 0)
        {
            Debug.LogWarning($"No valid collider paths found on {name}");
            return;
        }

        int idx = 0;
        foreach (var path in allPaths)
        {
            if (path.Length < 3) continue;

            var path3 = path.Select(p => (Vector3)p).ToArray();

            var go = new GameObject(childPrefix + idx++);
            go.transform.SetParent(transform, false);
            if (markAsEditorOnly) go.tag = "EditorOnly";

            var sc = go.AddComponent<ShadowCaster2D>();
            sc.selfShadows = selfShadows;

            fiShapePath.SetValue(sc, path3);
            fiShapePathHash.SetValue(sc, DeterministicHash(path3));
            fiApplyToSortingLayers.SetValue(sc, GetLayerIds());

#if UNITY_EDITOR
            // Tell Unity this object has been modified
            Undo.RegisterCreatedObjectUndo(go, "Create Shadow Caster");
            EditorUtility.SetDirty(go);
            PrefabUtility.RecordPrefabInstancePropertyModifications(go);

            // Also mark the parent prefab root dirty
            EditorUtility.SetDirty(gameObject);
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);

            this.hideFlags = HideFlags.DontSaveInBuild;
            this.enabled = false;
            EditorUtility.SetDirty(this);
#endif
        }
    }

    List<Vector2[]> CollectPaths()
    {
        var paths = new List<Vector2[]>();

        // CompositeCollider2D
        var composite = GetComponent<CompositeCollider2D>();
        if (composite != null)
        {
            for (int i = 0; i < composite.pathCount; i++)
            {
                var pts = new Vector2[composite.GetPathPointCount(i)];
                composite.GetPath(i, pts);
                if (pts.Length >= 3) paths.Add(pts);
            }
        }

        // PolygonCollider2D
        foreach (var poly in GetComponents<PolygonCollider2D>())
        {
            for (int i = 0; i < poly.pathCount; i++)
            {
                var pts = poly.GetPath(i);
                if (pts.Length >= 3) paths.Add(pts);
            }
        }

        // BoxCollider2D → simple rect
        foreach (var box in GetComponents<BoxCollider2D>())
        {
            var r = box.size * 0.5f;
            var pts = new Vector2[]
            {
                new Vector2(-r.x, -r.y), new Vector2(r.x, -r.y), new Vector2(r.x, r.y), new Vector2(-r.x, r.y)
            };
            paths.Add(pts);
        }

        return paths;
    }

    int[] GetLayerIds()
    {
        var layers = SortingLayer.layers;
        var list = new List<int>();
        if (!string.IsNullOrEmpty(sortingLayerA))
        {
            var id = layers.FirstOrDefault(l => l.name == sortingLayerA).id;
            if (id != 0) list.Add(id);
        }
        if (!string.IsNullOrEmpty(sortingLayerB))
        {
            var id = layers.FirstOrDefault(l => l.name == sortingLayerB).id;
            if (id != 0 && !list.Contains(id)) list.Add(id);
        }
        return list.ToArray();
    }

    static int DeterministicHash(Vector3[] pts)
    {
        unchecked
        {
            int h = 17;
            foreach (var p in pts)
            {
                h = h * 31 + p.x.GetHashCode();
                h = h * 31 + p.y.GetHashCode();
                h = h * 31 + p.z.GetHashCode();
            }
            return h;
        }
    }

    [MenuItem("Tools/URP 2D/Bake All ShadowCasters in Scene")]
    public static void BakeAllInScene()
    {
        var creators = GameObject.FindObjectsByType<ShadowCaster2DCreator>(FindObjectsSortMode.None);
        foreach (var c in creators)
            c.Create();

        Debug.Log($"Baked shadow casters for {creators.Length} creator(s) in scene.");
    }
}

[CustomEditor(typeof(ShadowCaster2DCreator))]
public class ShadowCaster2DCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var t = (ShadowCaster2DCreator)target;
        DrawDefaultInspector();

        EditorGUILayout.HelpBox("Generates baked ShadowCaster2D children from Composite, Polygon, or Box colliders.\n" +
                                "This avoids runtime freezes caused by collider-sourced ShadowCasters.", MessageType.Info);

        if (GUILayout.Button("Bake Shadow Caster(s)"))
            t.Create();
        if (GUILayout.Button("Destroy All Shadow Caster(s)"))
            t.Cleanup();
    }
}
