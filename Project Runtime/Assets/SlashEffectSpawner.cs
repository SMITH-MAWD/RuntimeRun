using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class SlashEffectSpawner : MonoBehaviour
{
    [Tooltip("Optional folder to limit search. Leave empty to search the whole character (Boss).")]
    [SerializeField] private Transform effectsRoot;

    [SerializeField] private float activeDuration = 3f;

    private readonly Dictionary<string, GameObject> fxByEventName = new();
    private readonly Dictionary<int, Coroutine> deactivateJobs = new();
    private readonly Dictionary<Transform, Vector3> originalLocalPositions = new();
    private readonly Dictionary<Transform, Vector3> originalLocalScales = new();
    // Transforms whose SlashFX opted out of facing mirror; restore authored pose instead.
    private readonly HashSet<Transform> noMirrorTransforms = new();

    private SpriteRenderer facingRenderer;
    private bool lastFlipX;

    private static readonly Regex HitKeyPattern = new(
        @"^(HitVar\d+|Hit\d+)",
        RegexOptions.IgnoreCase);

    private void Awake()
    {
        facingRenderer = GetComponent<SpriteRenderer>();
        if (facingRenderer == null)
            facingRenderer = GetComponentInParent<SpriteRenderer>();

        RefreshEffects(disableOnRegister: true);
    }

    private void LateUpdate()
    {
        if (facingRenderer != null)
            SyncFacing(facingRenderer.flipX);
    }

    public void SyncFacing(bool flipX, bool force = false)
    {
        if (!force && flipX == lastFlipX && originalLocalPositions.Count > 0)
            return;

        lastFlipX = flipX;
        float sign = flipX ? -1f : 1f;

        foreach (KeyValuePair<Transform, Vector3> entry in originalLocalPositions)
        {
            Transform t = entry.Key;
            if (t == null)
                continue;

            Vector3 originPos = entry.Value;

            // Per-FX override: opted-out FX (SlashFX.mirrorWithFacing == false) keep their authored local pose regardless of the boss's facing.
            if (noMirrorTransforms.Contains(t))
            {
                t.localPosition = originPos;
                if (originalLocalScales.TryGetValue(t, out Vector3 rawScale))
                    t.localScale = rawScale;
                continue;
            }

            // Mirror around x=0 using Abs * sign so the FX always sits on the facing side
            t.localPosition = new Vector3(Mathf.Abs(originPos.x) * sign, originPos.y, originPos.z);

            if (originalLocalScales.TryGetValue(t, out Vector3 originScale))
            {
                Vector3 scale = SanitizeScale(originScale);
                t.localScale = new Vector3(
                    Mathf.Abs(scale.x) * sign,
                    scale.y,
                    scale.z);
            }
        }
    }

    private static Vector3 SanitizeScale(Vector3 scale)
    {
        float ax = Mathf.Abs(scale.x);
        float ay = Mathf.Abs(scale.y);
        float az = Mathf.Abs(scale.z);

        if (ay < 0.001f)
            scale.y = ax > 0.001f ? Mathf.Sign(scale.x == 0f ? 1f : scale.x) * ax : 1f;

        if (az < 0.001f)
            scale.z = ax > 0.001f ? Mathf.Abs(scale.z) : 1f;

        return scale;
    }

    private void CacheFacingTransform(Transform t)
    {
        if (t == null || originalLocalPositions.ContainsKey(t))
            return;

        originalLocalPositions[t] = t.localPosition;
        originalLocalScales[t] = SanitizeScale(t.localScale);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
            RefreshEffects(disableOnRegister: false);
    }
#endif

    [ContextMenu("Refresh Slash FX From Hierarchy")]
    public void RefreshEffects()
    {
        RefreshEffects(disableOnRegister: false);
    }

    private Transform GetCharacterRoot()
    {
        Animator animator = GetComponentInParent<Animator>();
        return animator != null ? animator.transform : transform;
    }

    private Transform GetSearchRoot()
    {
        return effectsRoot != null ? effectsRoot : GetCharacterRoot();
    }

    private void RefreshEffects(bool disableOnRegister)
    {
        fxByEventName.Clear();
        originalLocalPositions.Clear();
        originalLocalScales.Clear();
        noMirrorTransforms.Clear();
        lastFlipX = false;

        Transform searchRoot = GetSearchRoot();
        ScanHierarchy(searchRoot, disableOnRegister);

        if (fxByEventName.Count == 0 && effectsRoot != null)
        {
            Debug.LogWarning(
                "SlashEffectSpawner: effectsRoot has no Hit/HitVar slashes; searching whole character instead.",
                this);
            effectsRoot = null;
            ScanHierarchy(GetCharacterRoot(), disableOnRegister);
        }

        if (fxByEventName.Count == 0 && disableOnRegister)
        {
            Debug.LogWarning(
                "SlashEffectSpawner: no Hit/HitVar slashes found under "
                + GetSearchRoot().name
                + ". Name objects Hit1, Hit2, Hit3, HitVar1, HitVar2…",
                this);
        }
    }

    private void ScanHierarchy(Transform searchRoot, bool disableOnRegister)
    {
        foreach (SlashFX slash in searchRoot.GetComponentsInChildren<SlashFX>(true))
        {
            if (slash == null || IsExcludedName(slash.gameObject.name))
                continue;

            Register(slash.gameObject, GetEventKey(slash.EventName), disableOnRegister);
        }

        foreach (Transform t in searchRoot.GetComponentsInChildren<Transform>(true))
        {
            if (t == searchRoot || !IsHitSlashObject(t.gameObject))
                continue;

            if (HasHitSlashParent(t))
                continue;

            Register(t.gameObject, GetEventKey(t.name), disableOnRegister);
        }
    }

    private static bool HasHitSlashParent(Transform t)
    {
        Transform parent = t.parent;
        while (parent != null)
        {
            if (IsHitSlashObject(parent.gameObject))
                return true;

            parent = parent.parent;
        }

        return false;
    }

    private static bool IsExcludedName(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
            return true;

        string lower = objectName.ToLowerInvariant();
        return lower.Contains("smoke") || lower.Contains("dead") || lower.Contains("spawnpoint");
    }

    private static bool IsHitSlashObject(GameObject go)
    {
        if (go == null || IsExcludedName(go.name))
            return false;

        if (!go.GetComponentInChildren<ParticleSystem>(true))
            return false;

        return HitKeyPattern.IsMatch(go.name);
    }

    private static bool IsSupportedEventKey(string key)
    {
        return !string.IsNullOrEmpty(key) && HitKeyPattern.IsMatch(key);
    }

    private static string GetEventKey(string objectName)
    {
        Match match = HitKeyPattern.Match(objectName);
        return match.Success ? match.Groups[1].Value : objectName;
    }

    private void Register(GameObject fx, string key, bool disableOnRegister)
    {
        if (fx == null || string.IsNullOrEmpty(key) || !IsSupportedEventKey(key))
            return;

        if (!fxByEventName.ContainsKey(key))
        {
            fxByEventName[key] = fx;
            CacheFacingTransform(fx.transform);

            // Per-FX override: if a SlashFX is present and opted out, remember to skip mirror.
            // FX without a SlashFX component keep the default mirror behavior.
            SlashFX slash = fx.GetComponent<SlashFX>();
            if (slash != null && !slash.MirrorWithFacing)
                noMirrorTransforms.Add(fx.transform);

            DisableAutoPlay(fx);
            if (disableOnRegister)
                fx.SetActive(false);
        }
    }

    private static void DisableAutoPlay(GameObject fx)
    {
        foreach (ParticleSystem ps in fx.GetComponentsInChildren<ParticleSystem>(true))
        {
            ParticleSystem.MainModule main = ps.main;
            main.playOnAwake = false;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void OnSlashAnimationEvent(string eventName)
    {
        ActivateByKey(eventName);
    }

    public void DeactivateAllSlashes()
    {
        foreach (GameObject fx in fxByEventName.Values)
            DeactivateSlashObject(fx);
    }

    private void ActivateByKey(string key)
    {
        if (string.IsNullOrEmpty(key) || !IsSupportedEventKey(key))
            return;

        if (fxByEventName.Count == 0)
            RefreshEffects(disableOnRegister: true);

        if (fxByEventName.TryGetValue(key, out GameObject fx))
            Activate(fx);
        else
            Debug.LogWarning(
                $"SlashEffectSpawner: no slash named \"{key}\". Found: {string.Join(", ", fxByEventName.Keys)}",
                this);
    }

    private void Activate(GameObject fx)
    {
        if (fx == null)
            return;

        int id = fx.GetInstanceID();

        if (deactivateJobs.TryGetValue(id, out Coroutine running) && running != null)
            StopCoroutine(running);

        if (facingRenderer != null)
            SyncFacing(facingRenderer.flipX, force: true);

        EnsureParentsActive(fx.transform);
        fx.SetActive(true);
        PlayAllParticles(fx);
        deactivateJobs[id] = StartCoroutine(DeactivateAfterDelay(id));
    }

    private static void EnsureParentsActive(Transform t)
    {
        Transform current = t.parent;
        while (current != null)
        {
            if (!current.gameObject.activeSelf)
                current.gameObject.SetActive(true);

            current = current.parent;
        }
    }

    private IEnumerator DeactivateAfterDelay(int instanceId)
    {
        yield return new WaitForSeconds(activeDuration);

        GameObject fx = GetFxByInstanceId(instanceId);
        if (fx == null)
        {
            deactivateJobs.Remove(instanceId);
            yield break;
        }

        DeactivateSlashObject(fx);
        deactivateJobs.Remove(instanceId);
    }

    private void DeactivateSlashObject(GameObject fx)
    {
        if (fx == null)
            return;

        foreach (ParticleSystem ps in fx.GetComponentsInChildren<ParticleSystem>(true))
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        fx.SetActive(false);
    }

    private GameObject GetFxByInstanceId(int instanceId)
    {
        foreach (GameObject fx in fxByEventName.Values)
        {
            if (fx != null && fx.GetInstanceID() == instanceId)
                return fx;
        }

        return null;
    }

    private static void PlayAllParticles(GameObject root)
    {
        foreach (ParticleSystem ps in root.GetComponentsInChildren<ParticleSystem>(true))
        {
            ps.Clear(true);
            ps.Play(true);
        }
    }
}

