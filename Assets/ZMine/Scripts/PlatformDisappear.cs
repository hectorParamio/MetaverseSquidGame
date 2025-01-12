using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class PlatformDisappear : UdonSharpBehaviour
{
    private MeshRenderer platformRenderer;
    private GameObject childPrefab;
    [Header("Audio")]
    public AudioSource disappearSound;
    public AudioClip disappearClip;
    public AudioSource shootSound;
    public AudioSource shootSound2;
    public AudioSource shootSound3;

    void Start()
    {
        // Log this object's name and its parent
        Debug.Log($"[PlatformDisappear] Script is on object: {gameObject.name}");
        Debug.Log($"[PlatformDisappear] Parent object is: {transform.parent.name}");
        
        // Get the parent platform's renderer
        platformRenderer = transform.parent.GetComponent<MeshRenderer>();
        if (platformRenderer != null)
        {
            Debug.Log($"[PlatformDisappear] Found platform renderer on: {platformRenderer.gameObject.name}");
        }
        
        // Get the corresponding LED_Square (L or R) based on parent name
        string parentName = transform.parent.name;
        string ledName = parentName.Contains("L") ? "LED_SquareL" : "LED_SquareR";
        Transform ledSquare = transform.parent.Find(ledName);
        if (ledSquare != null)
        {
            childPrefab = ledSquare.gameObject;
            Debug.Log($"[PlatformDisappear] Found LED Square: {ledName} on {childPrefab.name}");
        }
        else
        {
            Debug.LogWarning($"[PlatformDisappear] Could not find LED Square named {ledName}");
        }

        // Setup audio
        if (disappearSound != null)
        {
            Debug.Log($"[PlatformDisappear] Audio Source is on: {disappearSound.gameObject.name}");
            disappearSound.playOnAwake = false;
            if (disappearClip != null)
            {
                Debug.Log($"[PlatformDisappear] Audio Clip assigned: {disappearClip.name}");
                disappearSound.clip = disappearClip;
            }
        }
        else
        {
            Debug.LogWarning("[PlatformDisappear] No Audio Source assigned!");
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (platformRenderer != null)
        {
            // Only disable if the parent platform has no active collider
            if (platformRenderer.GetComponent<Collider>().enabled == false)
            {
                platformRenderer.enabled = false;
                // Play random sound effect
                if (shootSound != null && shootSound2 != null && shootSound3 != null)
                {
                    int randomSound = Random.Range(0, 3); // Returns 0 or 1
                    if (randomSound == 0)
                    {
                        shootSound.PlayOneShot(shootSound.clip);
                    }
                    else if(randomSound == 1){
                        shootSound2.PlayOneShot(shootSound2.clip);
                    }
                    else
                    {
                        shootSound3.PlayOneShot(shootSound3.clip);
                    }
                }

                if (childPrefab != null)
                {
                    childPrefab.SetActive(false);
                }
            }
        }
    }
} 