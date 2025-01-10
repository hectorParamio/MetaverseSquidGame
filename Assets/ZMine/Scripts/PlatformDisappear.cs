using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class PlatformDisappear : UdonSharpBehaviour
{
    private MeshRenderer platformRenderer;
    private GameObject childPrefab;

    void Start()
    {
        // Get the parent platform's renderer
        platformRenderer = transform.parent.GetComponent<MeshRenderer>();
        
        // Get the corresponding LED_Square (L or R) based on parent name
        string parentName = transform.parent.name;
        string ledName = parentName.Contains("L") ? "LED_SquareL" : "LED_SquareR";
        Transform ledSquare = transform.parent.Find(ledName);
        if (ledSquare != null)
        {
            childPrefab = ledSquare.gameObject;
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
                
                if (childPrefab != null)
                {
                    childPrefab.SetActive(false);
                }
            }
        }
    }
} 