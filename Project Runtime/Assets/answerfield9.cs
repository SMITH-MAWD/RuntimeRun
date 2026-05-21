using UnityEngine;
using UnityEngine.UI;

public class answerfield9 : MonoBehaviour
{
    [Tooltip("InputField where player types their answer. If left empty, the script will try to find an InputField on the same GameObject.")]
    public InputField inputField;

    // Many acceptable answers – case‑insensitive matching
    private string[] correctAnswers = new string[]
    {
        "int",         // Java primitive keyword
        "Int",         // players might capitalise
        "INT",
        "integer",     // English word
        "Integer",     // Java wrapper class
        "INTEGER",
        "int ",
        " int",
        "integer ",
        " integer",
        "int;",
        "int();"       // some might think it's a method, we'll still accept
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

        inputField.onEndEdit.AddListener(OnSubmit);
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
        if (!timerStarted && !string.IsNullOrEmpty(text))
        {
            timerStarted = true;
            timetoanswer timer = Object.FindFirstObjectByType<timetoanswer>();
            if (timer != null)
                timer.StartTimer();
        }
    }

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
            timetoanswer timer = Object.FindFirstObjectByType<timetoanswer>();
            if (timer != null)
                timer.StopTimer();
            timerStarted = false;

            Debug.Log("answerfield: Correct answer entered. Revealing small platforms.");

            smallplatscrpt9[] platforms = Object.FindObjectsByType<smallplatscrpt9>(FindObjectsSortMode.None);
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

            inputField.text = string.Empty;
            inputField.DeactivateInputField();
        }
        else
        {
            Debug.Log("answerfield: Incorrect answer entered: '" + text + "'");
            errorscript errorTracker = Object.FindFirstObjectByType<errorscript>();
            if (errorTracker != null)
                errorTracker.IncrementError();
        }
    }

    private void OnDisable()
    {
        if (timerStarted)
        {
            timetoanswer timer = Object.FindFirstObjectByType<timetoanswer>();
            if (timer != null)
                timer.StopTimer();
            timerStarted = false;
        }
    }
}