using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class MoveTimeline : MonoBehaviour
{
    [Header("Timelines to play in order")]
    [SerializeField] private List<TimelineAsset> timelines = new List<TimelineAsset>();

    [Header("Directors (one per timeline, in the same order)")]
    [SerializeField] private List<PlayableDirector> directors = new List<PlayableDirector>();

    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool playOnlyOnce = true;

    [Header("Answer System")]
    [SerializeField] private string correctAnswer = "ANSWER";
    [SerializeField] private GameObject effectPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool alsoPlayTimelinesOnCorrectAnswer = true;

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

    public void CheckAnswer(string userInput)
    {
        if (string.IsNullOrEmpty(userInput))
            return;

        if (userInput.Trim().Equals(correctAnswer.Trim(), System.StringComparison.OrdinalIgnoreCase))
        {
            if (effectPrefab != null)
            {
                Transform parent = spawnPoint ? spawnPoint : transform;
                Instantiate(effectPrefab, parent.position, parent.rotation);
            }

            if (alsoPlayTimelinesOnCorrectAnswer && !hasPlayed)
            {
                hasPlayed = playOnlyOnce;
                StartCoroutine(PlayTimelineSequence());
            }
        }
    }

    private IEnumerator PlayTimelineSequence()
    {
        for (int i = 0; i < timelines.Count; i++)
        {
            if (timelines[i] == null)
            {
                Debug.LogWarning($"Timeline at index {i} is null, skipping.", this);
                continue;
            }

            if (i >= directors.Count || directors[i] == null)
            {
                Debug.LogError($"No director assigned for timeline at index {i}.", this);
                yield break;
            }

            directors[i].playableAsset = timelines[i];
            directors[i].Play();

            while (directors[i].state == PlayState.Playing)
            {
                yield return null;
            }
        }
    }
}