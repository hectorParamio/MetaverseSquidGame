using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class PlatformToggle : UdonSharpBehaviour
{
    private bool initialized = false;
    [UdonSynced] private bool isLeftEnabled = true;

    void Start()
    {
        
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

        }

        // Then proceed with network initialization if owner
        if (Networking.IsOwner(gameObject))
        {
            InitializePlatforms();
        }
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (!initialized)
        {
            SyncPlatformState();
        }
    }

    private void InitializePlatforms()
    {
        if (!initialized)
        {
            int randomChoice = Random.Range(0, 2);
            isLeftEnabled = (randomChoice == 0);
            
            RequestSerialization();
            SyncPlatformState();
            
            initialized = true;
        }
    }

    public override void OnDeserialization()
    {
        SyncPlatformState();
    }

    private void SyncPlatformState()
    {
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