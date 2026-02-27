using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class errorscript : MonoBehaviour
{
    private int errorCount = 0;
    private TextMeshProUGUI errorDisplayText;

    void Start()
    {

        errorDisplayText = GetComponent<TextMeshProUGUI>();
        if (errorDisplayText == null)
        {
            Debug.LogWarning("errorscript: No TextMeshProUGUI component found on " + gameObject.name + ". Error count won't be displayed.");
        }

        UpdateDisplay();
    }


    // adds one to the error count 
    public void IncrementError()
    {
        errorCount++;
        Debug.Log("errorscript: Error count incremented to " + errorCount);
        UpdateDisplay();
    }

    // updates the text display
    private void UpdateDisplay()
    {
        if (errorDisplayText != null)
        {
            errorDisplayText.text = "Errors: " + errorCount;
        }
    }

    // getting the error count, maybe? it works so i wont touch it anymore
    public int GetErrorCount()
    {
        return errorCount;
    }
}
