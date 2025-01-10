using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class DyingFell : UdonSharpBehaviour
{
    public Transform respawnPoint; // Assign this in the inspector to your RespawnPoint

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        Debug.Log("[Dying] Player fell, teleporting to respawn point.");
        player.TeleportTo(respawnPoint.position, respawnPoint.rotation);
    }
} 