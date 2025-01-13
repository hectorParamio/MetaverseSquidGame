using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

public class DoorController : UdonSharpBehaviour
{
    public Transform doorLeft;
    public Transform doorRight;
    public float slideDistance = 5f;
    public float speed = 3f;
    public float closeDelay = 2f;  // Configurable delay before closing
    public TextMeshProUGUI pressEText;  // Reference to the TextMeshPro text

    private bool playerInTrigger = false;
    private bool hasBeenOpened = false;
    private bool isOpening = false;
    private float closeTimer = 0f;
    private bool secondSoundPlayed = false;  // Add this with other private variables
    private float soundTimer = 0f;           // Add this with other private variables

    private Vector3 initialLeftPosition;
    private Vector3 initialRightPosition;
    [Header("Audio")]
    public AudioSource sound;
    public AudioSource song;

    [UdonSynced] private bool isDoorOpened = false;
    public CountdownTimer countdownTimer; // Reference to your countdown timer

    void Start()
    {
        if (doorLeft == null || doorRight == null)
        {
            Debug.LogError("[DoorController] Door transforms not assigned!");
            return;
        }

        if (pressEText == null)
        {
            Debug.LogError("[DoorController] PressE text not assigned!");
            return;
        }

        initialLeftPosition = doorLeft.localPosition;
        initialRightPosition = doorRight.localPosition;
        pressEText.enabled = false;  // Hide text at start

        if (countdownTimer == null)
        {
            Debug.LogError("[DoorController] CountdownTimer component reference is missing!");
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player == Networking.LocalPlayer && !hasBeenOpened)
        {
            Debug.Log("[DoorController] Player entered trigger zone");
            playerInTrigger = true;
            if (pressEText != null)
            {
                pressEText.enabled = true;  // Show text when player enters
            }
        }
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (player == Networking.LocalPlayer)
        {
            Debug.Log("[DoorController] Player exited trigger zone");
            playerInTrigger = false;
            if (pressEText != null)
            {
                pressEText.enabled = false;  // Hide text when player exits
            }
        }
    }

    void Update()
    {
        if (doorLeft == null || doorRight == null) return;

        if (!hasBeenOpened && playerInTrigger && Input.GetKeyDown(KeyCode.E))
        {
            song.PlayOneShot(song.clip);
            // Take ownership and sync across network
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            isDoorOpened = true;
            RequestSerialization();
            
            // Call the networked method that handles door opening and timer start
            OpenDoorForAll();
        }

        if (soundTimer > 0 && !secondSoundPlayed)
        {
            soundTimer -= Time.deltaTime;
            if (soundTimer <= 0)
            {
                if (sound != null)
                {
                    sound.PlayOneShot(sound.clip);
                    secondSoundPlayed = true;
                }
            }
        }

        if (isOpening)
        {
            doorLeft.localPosition = Vector3.Lerp(doorLeft.localPosition, 
                initialLeftPosition + new Vector3(-slideDistance, 0, 0), 
                Time.deltaTime * speed);
            doorRight.localPosition = Vector3.Lerp(doorRight.localPosition, 
                initialRightPosition + new Vector3(slideDistance, 0, 0), 
                Time.deltaTime * speed);

            if (closeTimer > 0)
            {
                closeTimer -= Time.deltaTime;
                if (closeTimer <= 0)
                {
                    Debug.Log("[DoorController] Auto-closing door");
                    isOpening = false;
                }
            }
        }
        else
        {
            doorLeft.localPosition = Vector3.Lerp(doorLeft.localPosition, 
                initialLeftPosition, 
                Time.deltaTime * speed);
            doorRight.localPosition = Vector3.Lerp(doorRight.localPosition, 
                initialRightPosition, 
                Time.deltaTime * speed);
        }
    }

    public void OpenDoorForAll()
    {
        if (sound != null)
        {
            sound.PlayOneShot(sound.clip);
            soundTimer = sound.clip.length;
        }

        Debug.Log("[DoorController] Opening door for all players");
        isOpening = true;
        hasBeenOpened = true;
        closeTimer = closeDelay;
        
        if (pressEText != null)
        {
            pressEText.enabled = false;
        }

        // Start the countdown timer
        if (countdownTimer != null)
        {
            Debug.Log("[DoorController] Triggering countdown timer start");
            countdownTimer.StartTimer();
        }
        else
        {
            Debug.LogError("[DoorController] CountdownTimer reference is missing!");
        }
    }

    public override void OnDeserialization()
    {
        if (isDoorOpened && !hasBeenOpened)
        {
            OpenDoorForAll();
        }
    }
}


