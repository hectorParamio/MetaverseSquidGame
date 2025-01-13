using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class GunDestroyTrigger : UdonSharpBehaviour
{
    private EndPlatform endPlatform;
    private Personas personasManager;

    void Start()
    {
        // Find the EndPlatform object
        GameObject endPlatformObj = GameObject.Find("EndPlatform");
        if (endPlatformObj != null)
        {
            endPlatform = endPlatformObj.GetComponent<EndPlatform>();
            if (endPlatform == null)
            {
                Debug.LogError("[GunDestroyTrigger] EndPlatform component not found!");
            }
        }
        else
        {
            Debug.LogError("[GunDestroyTrigger] EndPlatform object not found in scene!");
        }

        // Find Personas manager (following DyingFell's pattern)
        GameObject personasObj = GameObject.Find("Personas");
        if (personasObj != null)
        {
            personasManager = personasObj.GetComponent<Personas>();
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        Debug.Log($"[GunDestroyTrigger] Player {player.displayName} entered trigger");

        // Update player state (following DyingFell's pattern)
        if (personasManager != null)
        {
            personasManager.SetPlayerCubeState(player.displayName, true);
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "OnPlayerDeath");
        }

        // Send network event to destroy gun
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "DestroyGunGlobally");
    }

    public void OnPlayerDeath()
    {
        // This method will be called on all clients when a player dies
        if (personasManager != null)
        {
            personasManager.SetPlayerCubeState(Networking.LocalPlayer.displayName, true);
        }
    }

    public void DestroyGunGlobally()
    {
        if (endPlatform != null)
        {
            endPlatform.DestroyGun();
            Debug.Log("[GunDestroyTrigger] Gun destroyed globally");
        }
    }
}