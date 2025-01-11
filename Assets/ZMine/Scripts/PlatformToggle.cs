using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class PlatformToggle : UdonSharpBehaviour
{
    private bool initialized = false;
    [UdonSynced] private bool isLeftEnabled = true;

    void Start()
    {
        Debug.Log("[PlatformToggle] Starting initialization");
        
        // First ensure both platforms are visible
        Transform leftPlatform = transform.Find("JumpPlatformL");
        Transform rightPlatform = transform.Find("JumpPlatformR");

        if (leftPlatform != null && rightPlatform != null)
        {
            MeshRenderer leftRenderer = leftPlatform.GetComponent<MeshRenderer>();
            MeshRenderer rightRenderer = rightPlatform.GetComponent<MeshRenderer>();
            Collider leftCollider = leftPlatform.GetComponent<Collider>();
            Collider rightCollider = rightPlatform.GetComponent<Collider>();

            // Enable everything at start
            if (leftRenderer != null) leftRenderer.enabled = true;
            if (rightRenderer != null) rightRenderer.enabled = true;
            if (leftCollider != null) leftCollider.enabled = true;
            if (rightCollider != null) rightCollider.enabled = true;

            Debug.Log("[PlatformToggle] Both platforms enabled initially");
        }

        // Then proceed with network initialization if owner
        if (Networking.IsOwner(gameObject))
        {
            Debug.Log("[PlatformToggle] Is owner, initializing platforms");
            InitializePlatforms();
        }
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        Debug.Log("[PlatformToggle] Player joined, initialized: " + initialized);
        if (!initialized)
        {
            SyncPlatformState();
        }
    }

    private void InitializePlatforms()
    {
        if (!initialized)
        {
            Debug.Log("[PlatformToggle] Performing initial platform setup");
            int randomChoice = Random.Range(0, 2);
            isLeftEnabled = (randomChoice == 0);
            Debug.Log("[PlatformToggle] Random choice made: Left platform enabled = " + isLeftEnabled);
            
            RequestSerialization();
            SyncPlatformState();
            
            initialized = true;
        }
    }

    public override void OnDeserialization()
    {
        Debug.Log("[PlatformToggle] Deserializing state: Left platform enabled = " + isLeftEnabled);
        SyncPlatformState();
    }

    private void SyncPlatformState()
    {
        Debug.Log("[PlatformToggle] Syncing platform state: Left platform enabled = " + isLeftEnabled);
        if (isLeftEnabled)
        {
            EnableLeftDisableRight();
        }
        else
        {
            EnableRightDisableLeft();
        }
    }

    public void EnableLeftDisableRight()
    {
        Transform leftPlatform = transform.Find("JumpPlatformL");
        Transform rightPlatform = transform.Find("JumpPlatformR");

        if (leftPlatform != null && rightPlatform != null)
        {
            Debug.Log("[PlatformToggle] Enabling Left, Disabling Right");
            Collider leftCollider = leftPlatform.GetComponent<Collider>();
            Collider rightCollider = rightPlatform.GetComponent<Collider>();
            MeshRenderer leftRenderer = leftPlatform.GetComponent<MeshRenderer>();
            MeshRenderer rightRenderer = rightPlatform.GetComponent<MeshRenderer>();

            if (leftCollider != null && rightCollider != null)
            {
                leftCollider.enabled = true;
                rightCollider.enabled = false;

            }
        }
    }

    public void EnableRightDisableLeft()
    {
        Transform leftPlatform = transform.Find("JumpPlatformL");
        Transform rightPlatform = transform.Find("JumpPlatformR");

        if (leftPlatform != null && rightPlatform != null)
        {
            Debug.Log("[PlatformToggle] Enabling Right, Disabling Left");
            Collider leftCollider = leftPlatform.GetComponent<Collider>();
            Collider rightCollider = rightPlatform.GetComponent<Collider>();
            MeshRenderer leftRenderer = leftPlatform.GetComponent<MeshRenderer>();
            MeshRenderer rightRenderer = rightPlatform.GetComponent<MeshRenderer>();

            if (leftCollider != null && rightCollider != null)
            {
                leftCollider.enabled = false;
                rightCollider.enabled = true;
            }
        }
    }
}