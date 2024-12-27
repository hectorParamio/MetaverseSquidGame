using UdonSharp;
using UnityEngine;
using TMPro;

public class CountdownTimer : UdonSharpBehaviour
{
    public TextMeshProUGUI timerText;
    public float countdownTime = 300f;

    private float timeRemaining;

    void Start()
    {
        timeRemaining = countdownTime;
        UpdateTimerDisplay();
    }

    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerDisplay();
        }
        else
        {
            timeRemaining = 0;
            UpdateTimerDisplay();
        }
    }

    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
