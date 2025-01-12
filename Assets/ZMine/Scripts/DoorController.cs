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

    private Vector3 initialLeftPosition;
    private Vector3 initialRightPosition;

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
            Debug.Log("[DoorController] Opening door");
            isOpening = true;
            hasBeenOpened = true;
            closeTimer = closeDelay;
            if (pressEText != null)
            {
                pressEText.enabled = false;  // Hide text when door is activated
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
}


