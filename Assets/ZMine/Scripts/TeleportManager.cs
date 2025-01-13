using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TeleportManager : UdonSharpBehaviour
{
    public Transform respawnPoint;
    [UdonSynced] private string playerToTeleport = "";

    void Start()
    {
        Debug.Log($"[TeleportManager] Component attached to GameObject: {gameObject.name}");
        
        if (respawnPoint == null)
        {
            GameObject respawnObj = GameObject.Find("RespawnPoint");
            if (respawnObj != null)
            {
                respawnPoint = respawnObj.transform;
            }
        }
    }

    public void TeleportPlayer(string playerName)
    {
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        
        playerToTeleport = playerName;
        RequestSerialization();
        
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "NetworkedTeleport");
    }

    public void NetworkedTeleport()
    {
        if (string.IsNullOrEmpty(playerToTeleport)) return;
        
        Debug.Log("[TeleportManager] NetworkedTeleport called for player: " + playerToTeleport);
        
        VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        VRCPlayerApi.GetPlayers(players);
        
        foreach (VRCPlayerApi player in players)
        {
            if (player != null && player.displayName == playerToTeleport)
            {
                Debug.Log("[TeleportManager] Teleporting player: " + playerToTeleport);
                player.TeleportTo(respawnPoint.position, respawnPoint.rotation);
                break;
            }
        }
    }
}
