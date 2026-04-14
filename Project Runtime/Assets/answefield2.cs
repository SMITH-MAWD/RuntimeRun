using UnityEngine;
using UnityEngine.UI;

public class answerfield2 : MonoBehaviour
{
    [Tooltip("InputField where player types their answer. If left empty, the script will try to find an InputField on the same GameObject.")]
    public InputField inputField;

    // private const Set answers = new Set("TAGS", "tags", "Elements");
    // if (answers.Has(text.Trim().ToUppercase()))
    // answer that reveals the platform, very choosy and needs to be specific 
    private const string correctAnswer = "TAGS";
    private const string correctAnswer2 = "tags";
    private const string correctAnswer3 = "Tags";
    private const string correctAnswer4 = "Elements";
    private const string correctAnswer5 = "elements";
    private const string correctAnswer6 = "ELEMENTS";

    private const string correctAnswer7 = "tag";
    private const string correctAnswer8 = "Tag";
    private const string correctAnswer9 = "TAG";

    private const string correctAnswer10 = "element";
    private const string correctAnswer11 = "Element";
    private const string correctAnswer12 = "ELEMENT";

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

        if (text.Trim().Equals(correctAnswer, System.StringComparison.OrdinalIgnoreCase) || text.Trim().Equals(correctAnswer2, System.StringComparison.OrdinalIgnoreCase) || text.Trim().Equals(correctAnswer3, System.StringComparison.OrdinalIgnoreCase) || text.Trim().Equals(correctAnswer4, System.StringComparison.OrdinalIgnoreCase) || text.Trim().Equals(correctAnswer5, System.StringComparison.OrdinalIgnoreCase) || text.Trim().Equals(correctAnswer6, System.StringComparison.OrdinalIgnoreCase) || text.Trim().Equals(correctAnswer7, System.StringComparison.OrdinalIgnoreCase) || text.Trim().Equals(correctAnswer8, System.StringComparison.OrdinalIgnoreCase) || text.Trim().Equals(correctAnswer9, System.StringComparison.OrdinalIgnoreCase) || text.Trim().Equals(correctAnswer10, System.StringComparison.OrdinalIgnoreCase) || text.Trim().Equals(correctAnswer11, System.StringComparison.OrdinalIgnoreCase) || text.Trim().Equals(correctAnswer12, System.StringComparison.OrdinalIgnoreCase))
        {
            // Stop timer on correct answer
            timetoanswer timer = Object.FindFirstObjectByType<timetoanswer>();
            if (timer != null)
                timer.StopTimer();
            timerStarted = false;

            Debug.Log("answerfield: Correct answer entered. Revealing small platforms.");


            smallplatscrpt2[] platforms = Object.FindObjectsByType<smallplatscrpt2>(FindObjectsSortMode.None);
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
