using UnityEngine;
using UnityEngine.Playables;

[DisallowMultipleComponent]
public class Play_Ifttrue : MonoBehaviour
{
    [Header("Playable Director to play when triggered")]
    [SerializeField] private PlayableDirector director;

    [Header("Optional: disable director on start (prevents auto-play)")]
    [SerializeField] private bool disableOnStart = true;

    void Awake()
    {
        if (disableOnStart && director != null)
            director.playOnAwake = false;
    }

    /// <summary>
    /// Call this to play the assigned timeline.
    /// </summary>
    public void TriggerTimeline()
    {
        if (director == null)
        {
            Debug.LogWarning($"Play_Ifttrue: No PlayableDirector assigned on '{name}'.");
            return;
        }

        director.Play();
        Debug.Log($"Play_Ifttrue: Played timeline on '{name}'.");
    }
}
