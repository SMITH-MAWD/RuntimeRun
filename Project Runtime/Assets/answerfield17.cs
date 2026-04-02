using UnityEngine;
using UnityEngine.UI;

public class answerfield17 : MonoBehaviour
{
    [Tooltip("InputField where player types their answer. If left empty, the script will try to find an InputField on the same GameObject.")]
    public InputField inputField;

    // answer that reveals the platform, very choosy and needs to be specific 
    private const string correctAnswer = "SCALABLE VECTOR GRAPHICS";
    private const string correctAnswer2 = "scalable vector graphics";
    private const string correctAnswer3 = "Scalable Vector Graphics";
    private const string correctAnswer4 = "SCALABLE VECTOR GRAPHIC";
    private const string correctAnswer5 = "scalable vector graphic";
    private const string correctAnswer6 = "Scalable Vector Graphic";
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

        if (text.Trim().Equals(correctAnswer, System.StringComparison.OrdinalIgnoreCase) || text.Trim().Equals(correctAnswer2, System.StringComparison.OrdinalIgnoreCase) || text.Trim().Equals(correctAnswer3, System.StringComparison.OrdinalIgnoreCase) || text.Trim().Equals(correctAnswer4, System.StringComparison.OrdinalIgnoreCase) || text.Trim().Equals(correctAnswer5, System.StringComparison.OrdinalIgnoreCase) || text.Trim().Equals(correctAnswer6, System.StringComparison.OrdinalIgnoreCase))
        {
            // Stop timer on correct answer
            timetoanswer timer = Object.FindFirstObjectByType<timetoanswer>();
            if (timer != null)
                timer.StopTimer();
            timerStarted = false;

            Debug.Log("answerfield: Correct answer entered. Revealing small platforms.");


            smallplatscrpt17[] platforms = Object.FindObjectsByType<smallplatscrpt17>(FindObjectsSortMode.None);
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
