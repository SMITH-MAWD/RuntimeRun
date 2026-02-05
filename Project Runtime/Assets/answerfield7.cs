using UnityEngine;
using UnityEngine.UI;

public class answerfield7 : MonoBehaviour
{
    [Tooltip("InputField where player types their answer. If left empty, the script will try to find an InputField on the same GameObject.")]
    public InputField inputField;

    // answer that reveals the platform, very choosy and needs to be specific 
    private const string correctAnswer = "answertest";

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

    }

    // when player finishes answering or closes the console
    public void OnSubmit(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        if (text.Trim().Equals(correctAnswer, System.StringComparison.OrdinalIgnoreCase))
        {

            smallplatscrpt7[] platforms = Object.FindObjectsByType<smallplatscrpt7>(FindObjectsSortMode.None);
            if (platforms != null && platforms.Length > 0)
            {
                foreach (var p in platforms)
                {
                    p.Reveal();
                }
            }
            else
            {
                Debug.LogWarning("answerfield: No smallplatscrpt2 instances found in the scene.");
            }

            // clear the field when input correct 
            inputField.text = string.Empty;
            inputField.DeactivateInputField();
        }
    }
}

