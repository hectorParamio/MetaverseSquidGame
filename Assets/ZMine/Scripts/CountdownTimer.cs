﻿using UdonSharp;
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
    [UdonSynced] private bool shouldChangeLightColor = false;
    public float timeRemaining { get; private set; }

    [Header("Light Settings")]
    [Tooltip("Reference to the Area Light that will change color")]
    public Light areaLight;
    private readonly Color timerEndColor = new Color(0.13f, 1f, 0f); // hex 21FF00 converted to RGB

    private void Start()
    {
        if (timerDisplay == null)
        {
            return;
        }
        timeRemaining = startTime;
        UpdateTimerDisplay();
        
        if (areaLight == null)
        {
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
                    shouldChangeLightColor = true;
                    RequestSerialization();
                }
                else
                {
                    RequestSerialization();
                }
            }
            
            // Update local display for all clients
            timeRemaining = networkTime;
            UpdateTimerDisplay();
        }

        // Check for light color change
        if (shouldChangeLightColor)
        {
            ChangeAreaLightColor();
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
        
        // Check if we should change the light color after deserialization
        if (shouldChangeLightColor)
        {
            ChangeAreaLightColor();
        }
    }

    private void ChangeAreaLightColor()
    {
        if (areaLight != null)
        {
            areaLight.color = timerEndColor;
        }
    }
}
