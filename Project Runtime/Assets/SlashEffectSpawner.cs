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

    private static readonly Regex HitKeyPattern = new(@"^(Hit\d+)", RegexOptions.IgnoreCase);

    private void Awake()
    {
        RefreshEffects(disableOnRegister: true);
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
                + "Name slash objects Hit1, Hit2… with particle effects under "
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
            DisableAutoPlay(fx);
            if (disableOnRegister)
                fx.SetActive(false);
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
