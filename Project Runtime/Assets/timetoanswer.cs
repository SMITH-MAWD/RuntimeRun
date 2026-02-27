using UnityEngine;
using UnityEngine.UI;

public class timetoanswer : MonoBehaviour
{
    private float timerDuration = 0f;
    private bool timerActive = false;
    private Text timerDisplayText;

    void Start()
    {
        // finding a text component
        // oh god this was easier
        timerDisplayText = GetComponent<Text>();
        if (timerDisplayText == null)
        {
            Debug.LogWarning("timetoanswer: No Text component found on " + gameObject.name + ". Timer won't be displayed.");
        }

        UpdateDisplay();
    }

    void Update()
    {
        if (timerActive)
        {
            timerDuration += Time.deltaTime;
            UpdateDisplay();
        }
    }

    // starts the timer 
    public void StartTimer()
    {
        timerDuration = 0f;
        timerActive = true;
        Debug.Log("timetoanswer: Timer started");
    }


    /// stop the timer and returns the like time it took to answer
    public float StopTimer()
    {
        timerActive = false;
        Debug.Log("timetoanswer: Timer stopped. Elapsed: " + timerDuration.ToString("F2") + "s");
        return timerDuration;
    }


    /// Update the timer displayed
    private void UpdateDisplay()
    {
        if (timerDisplayText != null)
        {
            timerDisplayText.text = "Time: " + timerDuration.ToString("F2") + "s";
        }
    }


    //    gets the elapsed time
    public float GetElapsedTime()
    {
        return timerDuration;
    }
}
