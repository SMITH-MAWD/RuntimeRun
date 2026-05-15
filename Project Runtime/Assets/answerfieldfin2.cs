using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using UnityEngine.Events;   // <-- added for the event

public class answerfieldfin2 : MonoBehaviour
{
    [Tooltip("InputField where player types their answer. If left empty, the script will try to find an InputField on the same GameObject.")]
    public InputField inputField;

    [Header("Timeline")]
    [Tooltip("Assign one or more Play_Ifttrue components to trigger when the correct answer is entered.")]
    [SerializeField] private Play_Ifttrue[] timelineControllers = new Play_Ifttrue[0];

    // --- NEW: Event that other scripts can listen to ---
    [Header("Events")]
    public UnityEvent OnCorrectAnswer;

    // Exact expected answer
    private const string correctAnswer = "False";

    private bool timerStarted = false;
    private bool hasAnswered = false;

    void Start()
    {
        if (inputField == null)
            inputField = GetComponent<InputField>();

        if (inputField == null)
        {
            Debug.LogWarning("answerfieldfin2: No InputField assigned or found on " + gameObject.name);
            return;
        }

        inputField.onEndEdit.AddListener(OnSubmit);
        inputField.onValueChanged.AddListener(OnValueChanged);

        if (timelineControllers == null || timelineControllers.Length == 0)
        {
            timelineControllers = Object.FindObjectsOfType<Play_Ifttrue>();
            if (timelineControllers == null) timelineControllers = new Play_Ifttrue[0];
        }
    }

    private void OnDestroy()
    {
        if (inputField != null)
        {
            inputField.onEndEdit.RemoveListener(OnSubmit);
            inputField.onValueChanged.RemoveListener(OnValueChanged);
        }
    }

    private void OnValueChanged(string text)
    {
        if (!timerStarted && !string.IsNullOrEmpty(text))
        {
            timerStarted = true;
            var timer = Object.FindFirstObjectByType<timetoanswer>();
            if (timer != null) timer.StartTimer();
        }
    }

    public void OnSubmit(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        if (text.Trim().Equals(correctAnswer, System.StringComparison.OrdinalIgnoreCase))
        {
            var timer = Object.FindFirstObjectByType<timetoanswer>();
            if (timer != null) timer.StopTimer();
            timerStarted = false;

            if (hasAnswered)
            {
                Debug.Log("answerfieldfin2: Already answered; skipping effects.");
                inputField.text = string.Empty;
                inputField.DeactivateInputField();
                return;
            }

            Debug.Log("answerfieldfin2: Correct answer entered.");

            // --- Invoke the event (so any listener, like correctanswernephew, gets notified) ---
            OnCorrectAnswer?.Invoke();

            // Existing platform reveal
            var platforms = FindObjectsOfType<smallplatscrpt16>();
            if (platforms != null && platforms.Length > 0)
            {
                foreach (var p in platforms) p.Reveal();
            }
            else
            {
                Debug.LogWarning("answerfieldfin2: No smallplatscrpt16 instances found.");
            }

            // Existing timeline trigger
            if (timelineControllers != null && timelineControllers.Length > 0)
            {
                foreach (var ctl in timelineControllers)
                {
                    if (ctl != null) ctl.TriggerTimeline();
                }
            }
            else
            {
                var directors = Object.FindObjectsOfType<PlayableDirector>();
                if (directors != null && directors.Length > 0)
                {
                    foreach (var pd in directors) pd.Play();
                }
                else
                {
                    Debug.LogWarning("answerfieldfin2: No timeline controllers or directors found.");
                }
            }

            hasAnswered = true;

            inputField.text = string.Empty;
            inputField.DeactivateInputField();
        }
        else
        {
            Debug.Log($"answerfieldfin2: Incorrect answer: '{text}'");
            var errorTracker = Object.FindFirstObjectByType<errorscript>();
            if (errorTracker != null) errorTracker.IncrementError();
        }
    }

    private void OnDisable()
    {
        if (timerStarted)
        {
            var timer = Object.FindFirstObjectByType<timetoanswer>();
            if (timer != null) timer.StopTimer();
            timerStarted = false;
        }
    }
}