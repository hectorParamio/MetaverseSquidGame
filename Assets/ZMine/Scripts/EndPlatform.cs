using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class EndPlatform : UdonSharpBehaviour
{
    public GameObject gunPrefab; // Assign your gun model in Unity Inspector
    private int remainingShots = 3;
    private bool hasGun = false;
    private VRCPlayerApi localPlayer;

    void Start()
    {
        localPlayer = Networking.LocalPlayer;
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player == localPlayer && !hasGun)
        {
            hasGun = true;
            remainingShots = 3;
            // You would typically instantiate or enable your gun model here
            if (gunPrefab != null)
            {
                gunPrefab.SetActive(true);
            }
        }
    }

    public override void InputUse(bool value, VRC.Udon.Common.UdonInputEventArgs args)
    {
        if (!hasGun || !value || remainingShots <= 0) return;

        bool isVR = localPlayer.IsUserInVR();
        bool shouldShoot = (isVR && args.handType == VRC.Udon.Common.HandType.RIGHT)
                          || (!isVR && args.boolValue);

        if (shouldShoot)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (remainingShots > 0)
        {
            // Add shooting logic here (particle effects, raycasts, etc.)
            remainingShots--;

            if (remainingShots <= 0)
            {
                // Disable gun when out of ammo
                if (gunPrefab != null)
                {
                    gunPrefab.SetActive(false);
                }
                hasGun = false;
            }
        }
    }
}