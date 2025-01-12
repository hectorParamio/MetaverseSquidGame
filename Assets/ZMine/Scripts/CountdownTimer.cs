using UdonSharp;
using UnityEngine;
using TMPro;

public class CountdownTimer : UdonSharpBehaviour
{
    public TextMeshProUGUI timerText;
    public float countdownTime = 300f;
    public Color normalColor = Color.white;
    public Color dangerColor = Color.red;

    public float timeRemaining;
    private bool isInDangerZone = false;

    void Start()
    {
        timeRemaining = countdownTime;
        UpdateTimerDisplay();
        timerText.color = normalColor;
    }

    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerDisplay();
            CheckDangerZone();
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

    public void SetTime(float newTime)
    {
        timeRemaining = newTime;
        UpdateTimerDisplay();
        CheckDangerZone();
    }

    void CheckDangerZone()
    {
        if (timeRemaining <= 12f && !isInDangerZone)
        {
            isInDangerZone = true;
            timerText.color = dangerColor;
        }
        else if (timeRemaining > 12f && isInDangerZone)
        {
            isInDangerZone = false;
            timerText.color = normalColor;
        }
    }
}
