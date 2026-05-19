using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections;
using UnityEngine.Playables;

public class SimpleNPC : MonoBehaviour
{
    [Header("Dialogue")]
    public string[] lines = new string[] { "Hello!", "How are you?" };
    public float textSpeed = 0.03f;

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
    private int index;

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.Q) && !isTalking)
            StartDialogue();

        if (isTalking && Input.GetKeyDown(KeyCode.Space))
        {
            if (dialogueText.text == lines[index])
                NextLine();
            else
            {
                StopAllCoroutines();
                dialogueText.text = lines[index];
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerNear = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (isTalking) EndDialogue();
        }
    }

    // Made public for Timeline signals
    public void StartDialogue()
    {
        isTalking = true;
        dialoguePanel.SetActive(true);
        nameText.text = gameObject.name;
        if (speakTimeline != null) speakTimeline.Play();
        onDialogueOpen.Invoke();

        index = 0;
        dialogueText.text = "";
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        foreach (char c in lines[index].ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    // Made public for Timeline signals
    public void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            dialogueText.text = "";
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    // Made public for Timeline signals
    public void EndDialogue()
    {
        isTalking = false;
        dialoguePanel.SetActive(false);
        if (speakTimeline != null) speakTimeline.Stop();
        onDialogueClose.Invoke();
    }
}