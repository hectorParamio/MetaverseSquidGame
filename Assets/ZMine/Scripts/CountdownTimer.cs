using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using TMPro;

public class CountdownTimer : UdonSharpBehaviour
{
    [Header("Timer Display")]
    [Tooltip("Reference to the TextMeshPro component that shows the countdown")]
    public TextMeshProUGUI timerDisplay; // This should be the timer display, NOT the "Press E" text
    public float startTime = 180f; // 3 minutes
    [UdonSynced] private bool isTimerRunning = false;
    [UdonSynced] private float networkTime = 0f;
    public float timeRemaining { get; private set; }

    [Header("Light Settings")]
    [Tooltip("Reference to the Area Light that will change color")]
    public Light areaLight;
    private readonly Color timerEndColor = new Color(0.13f, 1f, 0f); // hex 21FF00 converted to RGB

    private void Start()
    {
        if (timerDisplay == null)
        {
            Debug.LogError("[CountdownTimer] Timer display TextMeshPro reference is missing!");
            return;
        }
        timeRemaining = startTime;
        UpdateTimerDisplay();
        
        if (areaLight == null)
        {
            Debug.LogError("[CountdownTimer] Area Light reference is missing!");
        }
    }

    public void StartTimer()
    {
        if (!isTimerRunning)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            isTimerRunning = true;
            networkTime = startTime;
            RequestSerialization();
        }
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            if (Networking.IsOwner(gameObject))
            {
                networkTime -= Time.deltaTime;
                if (networkTime <= 0)
                {
                    networkTime = 0;
                    isTimerRunning = false;
                    ChangeAreaLightColor();
                    RequestSerialization();
                }
                else
                {
                    // Request serialization every frame while timer is running
                    RequestSerialization();
                }
            }
            
            // Update local display for all clients
            timeRemaining = networkTime;
            UpdateTimerDisplay();
        }
    }

    public void SetTime(float newTime)
    {
        if (Networking.IsOwner(gameObject))
        {
            networkTime = newTime;
            RequestSerialization();
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerDisplay != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerDisplay.text = $"{minutes:00}:{seconds:00}";

            // Debug log to check if the display is updating
            Debug.Log($"[CountdownTimer] Updating display: {timerDisplay.text}");
        }
        else
        {
            Debug.LogError("[CountdownTimer] Timer text component is missing!");
        }
    }

    public override void OnDeserialization()
    {
        timeRemaining = networkTime;
        UpdateTimerDisplay();
    }

    private void ChangeAreaLightColor()
    {
        if (areaLight != null)
        {
            areaLight.color = timerEndColor;
        }
    }
}
