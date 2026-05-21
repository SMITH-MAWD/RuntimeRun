using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class answerfieldfin3 : MonoBehaviour
{
    [Tooltip("InputField where player types their answer. If left empty, the script will try to find an InputField on the same GameObject.")]
    public InputField inputField;

    [Header("Timeline")]
    [Tooltip("Assign one or more Play_Ifttrue components to trigger when the correct answer is entered.")]
    [SerializeField] private Play_Ifttrue[] timelineControllers = new Play_Ifttrue[0];

    // Exact expected answer
    private const string correctAnswer = "B";

    private bool timerStarted = false;
    private bool hasAnswered = false; // prevents re-triggering during this session

    void Start()
    {
        if (inputField == null)
            inputField = GetComponent<InputField>();

        if (inputField == null)
        {
            Debug.LogWarning("answerfieldfin3: No InputField assigned or found on " + gameObject.name);
            return;
        }

        inputField.onEndEdit.AddListener(OnSubmit);
        inputField.onValueChanged.AddListener(OnValueChanged);

        // Auto-assign timeline controllers if none provided in inspector
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
                Debug.Log("answerfieldfin3: Already answered; skipping timeline replay.");
                // still clear input so the player sees it accepted
                inputField.text = string.Empty;
                inputField.DeactivateInputField();
                return;
            }

            Debug.Log("answerfieldfin3: Correct answer entered. Revealing platforms and triggering timelines.");

            var platforms = FindObjectsOfType<smallplatscrpt16>();
            if (platforms != null && platforms.Length > 0)
            {
                foreach (var p in platforms) p.Reveal();
            }
            else
            {
                Debug.LogWarning("answerfieldfin3: No smallplatscrpt16 instances found in the scene.");
            }

            // Trigger all assigned timeline controllers
            if (timelineControllers != null && timelineControllers.Length > 0)
            {
                foreach (var ctl in timelineControllers)
                {
                    if (ctl != null)
                    {
                        ctl.TriggerTimeline();
                        Debug.Log($"answerfieldfin3: Triggered timeline controller '{ctl.name}'.");
                    }
                }
            }
            else
            {
                // fallback: play any PlayableDirector(s) in the scene
                var directors = Object.FindObjectsOfType<PlayableDirector>();
                if (directors != null && directors.Length > 0)
                {
                    foreach (var pd in directors)
                    {
                        pd.Play();
                        Debug.Log($"answerfieldfin3: Fallback played PlayableDirector '{pd.name}'.");
                    }
                }
                else
                {
                    Debug.LogWarning("answerfieldfin3: No timeline controllers or PlayableDirectors found to trigger.");
                }
            }

            hasAnswered = true; // prevent re-triggering during this session

            // clear the field when input correct
            inputField.text = string.Empty;
            inputField.DeactivateInputField();
        }
        else
        {
            Debug.Log($"answerfieldfin3: Incorrect answer entered: '{text}'");
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
