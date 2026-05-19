using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using TMPro;

public class SimpleNPC : MonoBehaviour
{
    [Header("Dialogue (type lines here)")]
    [TextArea(2, 4)]
    public string[] dialogueLines = new string[] { "Hello!", "How are you?" };

    [Header("Typing Settings")]
    public float typingSpeed = 0.03f;

    [Header("UI References (drag from Canvas)")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;

    [Header("Timeline (optional)")]
    public PlayableDirector speakTimeline;

    private bool isPlayerNear = false;
    private bool isTalking = false;
    private int currentLine = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private void Start()
    {
        Debug.Log($"[SimpleNPC] Script started on {gameObject.name}");
        if (dialoguePanel == null) Debug.LogError("[SimpleNPC] dialoguePanel is NOT assigned in Inspector!");
        if (speakerNameText == null) Debug.LogError("[SimpleNPC] speakerNameText is NOT assigned!");
        if (dialogueText == null) Debug.LogError("[SimpleNPC] dialogueText is NOT assigned!");
    }

    private void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("[SimpleNPC] Q pressed, player near = true. Starting dialogue...");
            if (!isTalking)
                StartDialogue();
            else
                Debug.Log("[SimpleNPC] Already talking, ignoring Q.");
        }

        if (isTalking && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("[SimpleNPC] Space pressed during dialogue.");
            NextOrSkip();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[SimpleNPC] Trigger entered by {other.name} with tag {other.tag}");
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            Debug.Log("[SimpleNPC] Player entered trigger zone.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            Debug.Log("[SimpleNPC] Player left trigger zone.");
            if (isTalking)
                EndDialogue();
        }
    }

    private void StartDialogue()
    {
        Debug.Log("[SimpleNPC] StartDialogue called.");
        isTalking = true;
        currentLine = 0;
        
        if (dialoguePanel == null)
        {
            Debug.LogError("[SimpleNPC] Cannot start dialogue: dialoguePanel is null!");
            return;
        }
        
        dialoguePanel.SetActive(true);
        Debug.Log("[SimpleNPC] Dialogue panel set active.");
        
        // Use the GameObject's name
        string nameToShow = gameObject.name;
        if (speakerNameText == null)
            Debug.LogError("[SimpleNPC] speakerNameText is null, cannot set name!");
        else
        {
            speakerNameText.text = nameToShow;
            Debug.Log($"[SimpleNPC] Set speaker name to '{nameToShow}'");
        }
        
        if (speakTimeline != null)
        {
            speakTimeline.Play();
            Debug.Log("[SimpleNPC] Timeline played.");
        }
        else
        {
            Debug.Log("[SimpleNPC] No timeline assigned, skipping.");
        }
        
        ShowLine();
    }

    private void ShowLine()
    {
        if (currentLine >= dialogueLines.Length)
        {
            Debug.Log("[SimpleNPC] No more lines, ending dialogue.");
            EndDialogue();
            return;
        }

        Debug.Log($"[SimpleNPC] Showing line {currentLine}: '{dialogueLines[currentLine]}'");
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        
        typingCoroutine = StartCoroutine(TypeText(dialogueLines[currentLine]));
    }

    private IEnumerator TypeText(string line)
    {
        isTyping = true;
        if (dialogueText == null)
        {
            Debug.LogError("[SimpleNPC] dialogueText is null, cannot type!");
            yield break;
        }
        dialogueText.text = "";
        Debug.Log("[SimpleNPC] Starting typewriter effect...");
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
        Debug.Log("[SimpleNPC] Finished typing line.");
    }

    private void NextOrSkip()
    {
        if (isTyping)
        {
            Debug.Log("[SimpleNPC] Skipping typing...");
            StopCoroutine(typingCoroutine);
            if (dialogueText != null && currentLine < dialogueLines.Length)
                dialogueText.text = dialogueLines[currentLine];
            isTyping = false;
        }
        else
        {
            Debug.Log("[SimpleNPC] Moving to next line.");
            currentLine++;
            ShowLine();
        }
    }

    private void EndDialogue()
    {
        Debug.Log("[SimpleNPC] EndDialogue called.");
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        
        isTalking = false;
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        
        if (speakTimeline != null)
            speakTimeline.Stop();
    }
}