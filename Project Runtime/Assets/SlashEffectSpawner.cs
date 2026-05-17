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

    private SpriteRenderer facingRenderer;
    private bool lastFlipX;

    private static readonly Regex HitKeyPattern = new(
        @"^(HitVar\d+|Hit\d+|Dead\d+|Smoke\d+)",
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

        foreach (var entry in originalLocalPositions)
        {
            Transform t = entry.Key;
            if (t == null)
                continue;

            Vector3 originPos = entry.Value;
            t.localPosition = new Vector3(originPos.x * sign, originPos.y, originPos.z);

            if (originalLocalScales.TryGetValue(t, out Vector3 originScale))
            {
                t.localScale = new Vector3(
                    Mathf.Abs(originScale.x) * sign,
                    originScale.y,
                    originScale.z);
            }
        }
    }

    private void CacheFacingTransform(Transform t)
    {
        if (t == null || originalLocalPositions.ContainsKey(t))
            return;

        originalLocalPositions[t] = t.localPosition;
        originalLocalScales[t] = t.localScale;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        RefreshEffects(disableOnRegister: false);
    }
#endif

    [ContextMenu("Refresh Slash FX From Hierarchy")]
    public void RefreshEffects()
    {
        RefreshEffects(disableOnRegister: false);
    }

    private Transform GetSearchRoot()
    {
        if (effectsRoot != null)
            return effectsRoot;

        Animator animator = GetComponentInParent<Animator>();
        return animator != null ? animator.transform : transform;
    }

    private void RefreshEffects(bool disableOnRegister)
    {
        fxByEventName.Clear();

        Transform searchRoot = GetSearchRoot();

        foreach (SlashFX slash in searchRoot.GetComponentsInChildren<SlashFX>(true))
        {
            if (slash != null)
                Register(slash.gameObject, slash.EventName, disableOnRegister);
        }

        foreach (Transform t in searchRoot.GetComponentsInChildren<Transform>(true))
        {
            if (!LooksLikeSlashObject(t.gameObject))
                continue;

            Register(t.gameObject, GetEventKey(t.name), disableOnRegister);
        }

        if (fxByEventName.Count == 0 && disableOnRegister)
        {
            Debug.LogWarning(
                "SlashEffectSpawner: no slash FX found. Put SlashEffectSpawner on Boss (not on the slash). "
                + "Name slash objects Hit1 / HitVar1 / Dead1 etc. with particle effects under "
                + searchRoot.name + ".",
                this);
        }
    }

    private static string GetEventKey(string objectName)
    {
        Match match = HitKeyPattern.Match(objectName);
        return match.Success ? match.Groups[1].Value : objectName;
    }

    private static bool LooksLikeSlashObject(GameObject go)
    {
        if (!go.GetComponentInChildren<ParticleSystem>(true))
            return false;

        if (HitKeyPattern.IsMatch(go.name))
            return true;

        return go.name.Contains("Slash") && !go.name.Contains("SpawnPoint");
    }

    private void Register(GameObject fx, string key, bool disableOnRegister)
    {
        if (fx == null || string.IsNullOrEmpty(key))
            return;

        if (!fxByEventName.ContainsKey(key))
        {
            fxByEventName[key] = fx;
            CacheFacingTransform(fx.transform);
            DisableAutoPlay(fx);
            if (disableOnRegister)
                fx.SetActive(false);

            if (facingRenderer != null)
                SyncFacing(facingRenderer.flipX);
        }
    }

    private static void DisableAutoPlay(GameObject fx)
    {
        foreach (ParticleSystem ps in fx.GetComponentsInChildren<ParticleSystem>(true))
        {
            var main = ps.main;
            main.playOnAwake = false;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void OnSlashAnimationEvent(string eventName)
    {
        ActivateByKey(eventName);
    }

    public void OnSlashAnimationEvent()
    {
    }

    public void DeactivateAllSlashes()
    {
        foreach (GameObject fx in fxByEventName.Values)
            DeactivateNow(fx);
    }

    private void ActivateByKey(string key)
    {
        if (string.IsNullOrEmpty(key))
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

        fx.SetActive(true);
        PlayAllParticles(fx);
        deactivateJobs[id] = StartCoroutine(DeactivateAfterDelay(id));
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

        DeactivateNow(fx);
        deactivateJobs.Remove(instanceId);
    }

    private void DeactivateNow(GameObject fx)
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
