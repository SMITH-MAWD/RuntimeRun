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
    public float textSpeed = 0.05f;          // Adjust for typing speed
    public bool useTalkCounter = true;
    [SerializeField] private int timesTalked = 0;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Timeline")]
    public PlayableDirector speakTimeline;

    [Header("Events")]
    public UnityEvent onDialogueOpen;
    public UnityEvent onDialogueClose;

    private bool isPlayerNear = false;
    private bool isTalking = false;
    private int currentLineIndex = 0;
    private string[] currentLines;

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
        {
            isPlayerNear = true;
        }
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

        // Choose dialogue set
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
}