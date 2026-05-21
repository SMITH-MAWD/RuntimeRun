using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class Movetimeline : MonoBehaviour
{
    [Header("Timelines to play in order")]
    [SerializeField] private List<TimelineAsset> timelines = new List<TimelineAsset>();

    [Header("Directors (one per timeline, in the same order)")]
    [SerializeField] private List<PlayableDirector> directors = new List<PlayableDirector>();

    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool playOnlyOnce = true;

    private bool hasPlayed = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasPlayed)
            return;

        if (other.CompareTag(playerTag))
        {
            hasPlayed = playOnlyOnce;
            StartCoroutine(PlayTimelineSequence());
        }
    }

    private IEnumerator PlayTimelineSequence()
    {
        for (int i = 0; i < timelines.Count; i++)
        {
            // Safety checks
            if (timelines[i] == null)
            {
                Debug.LogWarning($"Timeline at index {i} is null, skipping.", this);
                continue;
            }

            if (i >= directors.Count || directors[i] == null)
            {
                Debug.LogError($"No director assigned for timeline at index {i}. Make sure the Directors list has the same length as Timelines.", this);
                yield break;
            }

            // Play the timeline on its corresponding director
            directors[i].playableAsset = timelines[i];
            directors[i].Play();

            // Wait until it finishes
            while (directors[i].state == PlayState.Playing)
            {
                yield return null;
            }
        }
    }
}