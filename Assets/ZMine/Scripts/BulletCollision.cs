using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class BulletCollision : UdonSharpBehaviour
{
    public Transform respawnPoint;
    public bool isMalfunctioning = false; // Flag to check if the bullet is malfunctioning

    void Start()
    {
        GameObject respawnObj = GameObject.Find("RespawnPoint");
        if (respawnObj != null)
        {
            respawnPoint = respawnObj.transform;
            Debug.Log("[BulletCollision] Found RespawnPoint at: " + respawnPoint.position);
        }
        else
        {
            Debug.LogError("[BulletCollision] Could not find RespawnPoint in scene!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (respawnPoint == null) 
        {
            Debug.LogError("[BulletCollision] OnTriggerEnter - RespawnPoint is null!");
            return;
        }

        VRCPlayerApi player = Networking.GetOwner(other.gameObject);
        // if (player != null)
        // {
        //     if (isMalfunctioning && player.isLocal)
        //     {
        //         // Teleport the shooter if the bullet is malfunctioning and it's the local player
        //         Debug.Log("[BulletCollision] Malfunctioning bullet hit, teleporting shooter.");
        //         player.TeleportTo(respawnPoint.position, respawnPoint.rotation);
        //     }
        //     else
        //     {
        //         // Teleport the player hit by the bullet
        //         Debug.Log("[BulletCollision] Player hit by bullet, teleporting to respawn point: " + player);
        //         player.TeleportTo(respawnPoint.position, respawnPoint.rotation);
        //     }
        // }
    }
}