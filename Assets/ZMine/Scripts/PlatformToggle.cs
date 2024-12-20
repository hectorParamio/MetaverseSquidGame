using UdonSharp;
using UnityEngine;

public class PlatformToggle : UdonSharpBehaviour
{
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
                }
                else
                {
                    leftCollider.enabled = false;
                    rightCollider.enabled = true;
                }
            }
        }
    }
}