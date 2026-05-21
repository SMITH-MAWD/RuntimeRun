using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections;
using UnityEngine.Playables;

public class SimpleNPC : MonoBehaviour
{
    [Header("Dialogue Sets")]
    public string[] firstTimeLines = new string[] { "Hello stranger!", "Welcome to my shop." };
    public string[] repeatLines = new string[] { "Back again?", "What do you need?" };

    [Header("Settings")]
    public float textSpeed = 0.05f;
    public bool useTalkCounter = true;
    [SerializeField] private int timesTalked = 0;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Timeline")]
    public PlayableDirector speakTimeline;

    [Header("Audio")]
    public AudioClip typingSound;          // Assign a short blip sound (or first snippet of a voice)
    public AudioSource audioSource;        // Optional – if left empty, one will be created automatically
    [Range(0f, 0.5f)] public float playDuration = 0f; // If > 0, only play this many seconds of the clip (e.g., 0.05 for "first bit")

    [Header("Events")]
    public UnityEvent onDialogueOpen;
    public UnityEvent onDialogueClose;

    private bool isPlayerNear = false;
    private bool isTalking = false;
    private int currentLineIndex = 0;
    private string[] currentLines;

    void Awake()
    {
        // Create an AudioSource if none is assigned
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f; // 2D sound
        }
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.Q) && !isTalking)
            StartDialogue();

        if (isTalking && Input.GetKeyDown(KeyCode.Space))
        {
            if (dialogueText.text == currentLines[currentLineIndex])
                NextLine();
            else
            {
                StopAllCoroutines();
                dialogueText.text = currentLines[currentLineIndex];
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerNear = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (isTalking) EndDialogue();
        }
    }

    public void StartDialogue()
    {
        if (dialoguePanel == null) return;

        if (useTalkCounter && timesTalked > 0)
            currentLines = repeatLines;
        else
            currentLines = firstTimeLines;

        if (currentLines == null || currentLines.Length == 0)
            currentLines = new string[] { "Hello." };

        isTalking = true;
        dialoguePanel.SetActive(true);
        nameText.text = gameObject.name;

        if (speakTimeline != null) speakTimeline.Play();
        onDialogueOpen.Invoke();

        currentLineIndex = 0;
        dialogueText.text = "";
        StartCoroutine(TypeLine(currentLines[currentLineIndex]));

        if (useTalkCounter) timesTalked++;
    }

    IEnumerator TypeLine(string line)
    {
        dialogueText.text = "";
        foreach (char c in line)
        {
            dialogueText.text += c;
            PlayTypingSound(); // <-- Added: sound on each character
            yield return new WaitForSeconds(textSpeed);
        }
    }

    public void NextLine()
    {
        if (currentLineIndex < currentLines.Length - 1)
        {
            currentLineIndex++;
            dialogueText.text = "";
            StartCoroutine(TypeLine(currentLines[currentLineIndex]));
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        isTalking = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (speakTimeline != null) speakTimeline.Stop();
        onDialogueClose.Invoke();
    }

    public void ResetTalkCount()
    {
        timesTalked = 0;
    }

    public void SetDialogueSet(string[] newLines)
    {
        firstTimeLines = newLines;
    }

    // --- Typing Sound Helpers ---
    private void PlayTypingSound()
    {
        if (typingSound == null || audioSource == null) return;

        if (playDuration > 0f)
        {
            // Play only the first "playDuration" seconds of the clip
            audioSource.clip = typingSound;
            audioSource.time = 0f;
            audioSource.Play();
            StartCoroutine(StopAfterDuration(playDuration));
        }
        else
        {
            // Standard one‑shot – good for short blips that don't cut themselves off
            audioSource.PlayOneShot(typingSound);
        }
    }

    private IEnumerator StopAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (audioSource != null && audioSource.clip == typingSound)
            audioSource.Stop();
    }
}