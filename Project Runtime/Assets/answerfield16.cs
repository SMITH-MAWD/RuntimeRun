using UnityEngine;
using UnityEngine.UI;

public class answerfield16 : MonoBehaviour
{
    [Tooltip("InputField where player types their answer. If left empty, the script will try to find an InputField on the same GameObject.")]
    public InputField inputField;

    // Multiple acceptable answers – feel free to add more in the Inspector or directly in code
    private string[] correctAnswers = new string[]
    {
        "SYSTEM.OUT.PRINT();",          // original very picky answer
        "System.out.print();",          // standard Java casing, with semicolon
        "System.out.print()",           // without semicolon
        "system.out.print();",
        "system.out.print()",
        "System.out.print(\"\");",      // with an empty string argument
        "System.out.print(\"\")",
        "System.out.print('');",
        "System.out.print('')",
        "System.out.print(\" \");",     // with a space
        "System.out.print(\" \")",
        "System.out.print(\"\");",      // etc.
        "System.out.print()",           // just the method signature
        "system.out.print ( )",         // spaces inside parentheses
        "System.out.print ();"
    };

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
        bool isCorrect = false;
        foreach (string answer in correctAnswers)
        {
            if (trimmed.Equals(answer, System.StringComparison.OrdinalIgnoreCase))
            {
                isCorrect = true;
                break;
            }
        }

        if (isCorrect)
        {
            // Stop timer on correct answer
            timetoanswer timer = Object.FindFirstObjectByType<timetoanswer>();
            if (timer != null)
                timer.StopTimer();
            timerStarted = false;

            Debug.Log("answerfield: Correct answer entered. Revealing small platforms.");

            smallplatscrpt16[] platforms = Object.FindObjectsByType<smallplatscrpt16>(FindObjectsSortMode.None);
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
        // if the answer field is turned off without any answer inputted stop the timer
        if (timerStarted)
        {
            timetoanswer timer = Object.FindFirstObjectByType<timetoanswer>();
            if (timer != null)
                timer.StopTimer();
            timerStarted = false;
        }
    }
}