using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class PlatformToggle : UdonSharpBehaviour
{
    private GameObject disabledPlatform;
    private MeshRenderer disabledPlatformRenderer;

    private void Start()
    {
        Transform leftPlatform = transform.Find("JumpPlatformL");
        Transform rightPlatform = transform.Find("JumpPlatformR");

        if (leftPlatform != null && rightPlatform != null)
        {
            int randomChoice = Random.Range(0, 2);
            Collider leftCollider = leftPlatform.GetComponent<Collider>();
            Collider rightCollider = rightPlatform.GetComponent<Collider>();

            if (leftCollider != null && rightCollider != null)
            {
                if (randomChoice == 0)
                {
                    leftCollider.enabled = true;
                    rightCollider.enabled = false;
                    disabledPlatform = rightPlatform.gameObject;
                }
                else
                {
                    leftCollider.enabled = false;
                    rightCollider.enabled = true;
                    disabledPlatform = leftPlatform.gameObject;
                }

                // Get the MeshRenderer of the disabled platform
                disabledPlatformRenderer = disabledPlatform.GetComponent<MeshRenderer>();
            }
        }
    }

}