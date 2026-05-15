using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using UnityEngine.Events;

public class answerfieldfin3 : MonoBehaviour, IAnswerCorrectNotifier
{
    public InputField inputField;

    [Header("Answer")]
    [SerializeField] private string correctAnswer = "False";   // now configurable per instance

    [Header("Timeline")]
    [SerializeField] private Play_Ifttrue[] timelineControllers = new Play_Ifttrue[0];

    public UnityEvent OnCorrectAnswer { get; private set; } = new UnityEvent();

    private bool timerStarted = false;
    private bool hasAnswered = false;

    void Start()
    {
        if (inputField == null)
            inputField = GetComponent<InputField>();

        if (inputField == null)
        {
            Debug.LogWarning("answerfieldfin3: No InputField on " + gameObject.name);
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

        if (text.Trim().Equals(correctAnswer.Trim(), System.StringComparison.OrdinalIgnoreCase))
        {
            var timer = Object.FindFirstObjectByType<timetoanswer>();
            if (timer != null) timer.StopTimer();
            timerStarted = false;

            if (hasAnswered)
            {
                inputField.text = string.Empty;
                inputField.DeactivateInputField();
                return;
            }

            Debug.Log($"{name}: Correct answer entered.");

            // Notify any listener (like correctanswernephew)
            OnCorrectAnswer?.Invoke();

            var platforms = FindObjectsOfType<smallplatscrpt16>();
            if (platforms != null && platforms.Length > 0)
            {
                foreach (var p in platforms) p.Reveal();
            }

            if (timelineControllers != null && timelineControllers.Length > 0)
            {
                foreach (var ctl in timelineControllers) ctl.TriggerTimeline();
            }
            else
            {
                var directors = FindObjectsOfType<PlayableDirector>();
                if (directors != null)
                {
                    foreach (var pd in directors) pd.Play();
                }
            }

            hasAnswered = true;
            inputField.text = string.Empty;
            inputField.DeactivateInputField();
        }
        else
        {
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