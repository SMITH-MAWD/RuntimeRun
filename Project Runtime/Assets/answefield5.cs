using UnityEngine;
using UnityEngine.UI;

public class answerfield5 : MonoBehaviour
{
    [Tooltip("InputField where player types their answer. If left empty, the script will try to find an InputField on the same GameObject.")]
    public InputField inputField;

    // answer that reveals the platform, very choosy and needs to be specific 
    private const string correctAnswer = "HTML ELEMENTS";
    private const string correctAnswer2 = "html elements";
    private const string correctAnswer3 = "HTML elements";
    private const string correctAnswer4 = "html Elements";
    private const string correctAnswer5 = "HTML Elements";
    private const string correctAnswer6 = "html Elements";
    private const string correctAnswer7 = "Tags";
    private const string correctAnswer8 = "TAGS";
    private const string correctAnswer9 = "HTML TAGS";
    private const string correctAnswer10 = "tags";
    private const string correctAnswer11 = "html tags";
    private const string correctAnswer12 = "HTML tags";
    private const string correctAnswer13 = "html Tags";
    private const string correctAnswer14 = "HTML Tags";
    private const string correctAnswer15 = "ELEMENTS";
    private const string correctAnswer16 = "elements";
    private const string correctAnswer17 = "Elements";


    void Start()
    {
        if (inputField == null)
            inputField = GetComponent<InputField>();

        if (inputField == null)
        {
            Debug.LogWarning("answerfield: No InputField assigned or found on " + gameObject.name + ". Attach this script to an InputField or assign one in the inspector.");
            return;
        }

        // submit handler
        inputField.onEndEdit.AddListener(OnSubmit);
        // player opens the field
        inputField.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnDestroy()
    {
        if (inputField != null)
        {
            inputField.onEndEdit.RemoveListener(OnSubmit);
            inputField.onValueChanged.RemoveListener(OnValueChanged);
        }
    }

    private bool timerStarted = false;

    private void OnValueChanged(string text)
    {
        // Start timer on first letter typed 
        if (!timerStarted && !string.IsNullOrEmpty(text))
        {
            timerStarted = true;
            timetoanswer timer = Object.FindFirstObjectByType<timetoanswer>();
            if (timer != null)
                timer.StartTimer();
        }
    }

    // when player finishes answering or closes the console
    public void OnSubmit(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        string trimmed = text.Trim();
        bool isCorrect = trimmed.Equals(correctAnswer, System.StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals(correctAnswer2, System.StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals(correctAnswer3, System.StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals(correctAnswer4, System.StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals(correctAnswer5, System.StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals(correctAnswer6, System.StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals(correctAnswer7, System.StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals(correctAnswer8, System.StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals(correctAnswer9, System.StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals(correctAnswer10, System.StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals(correctAnswer11, System.StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals(correctAnswer12, System.StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals(correctAnswer13, System.StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals(correctAnswer14, System.StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals(correctAnswer15, System.StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals(correctAnswer16, System.StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals(correctAnswer17, System.StringComparison.OrdinalIgnoreCase);

        if (isCorrect)
        {
            // Stop timer on correct answer
            timetoanswer timer = Object.FindFirstObjectByType<timetoanswer>();
            if (timer != null)
                timer.StopTimer();
            timerStarted = false;

            Debug.Log("answerfield: Correct answer entered. Revealing small platforms.");


            smallplatscrpt5[] platforms = Object.FindObjectsByType<smallplatscrpt5>(FindObjectsSortMode.None);
            if (platforms != null && platforms.Length > 0)
            {
                foreach (var p in platforms)
                {
                    p.Reveal();
                }
            }
            else
            {
                Debug.LogWarning("answerfield: No smallplatscrpt instances found in the scene.");
            }

            // clear the field when input correct 
            inputField.text = string.Empty;
            inputField.DeactivateInputField();
        }


        else
        {
            Debug.Log("answerfield: Incorrect answer entered: '" + text + "'");
            // increment error counter but DO NOT stop timer
            errorscript errorTracker = Object.FindFirstObjectByType<errorscript>();
            if (errorTracker != null)
                errorTracker.IncrementError();
        }
    }

    private void OnDisable()
    {
        // if the answer field is like turned off without any answer inputted stop the timer
        if (timerStarted)
        {
            timetoanswer timer = Object.FindFirstObjectByType<timetoanswer>();
            if (timer != null)
                timer.StopTimer();
            timerStarted = false;
        }
    }
}
