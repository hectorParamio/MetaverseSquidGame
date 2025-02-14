﻿using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using TMPro;

public class DyingFell : UdonSharpBehaviour
{
    public Transform respawnPoint;
    [SerializeField] private Material deadMaterial;
    private Personas personasManager;

    void Start()
    {
        // Find the Personas manager
        GameObject personasObj = GameObject.Find("Personas");
        if (personasObj != null)
        {
            personasManager = personasObj.GetComponent<Personas>();
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        Debug.Log($"[DyingFell] Player {player.displayName} fell, attempting to change cube state");

        if (personasManager != null)
        {
            personasManager.SetPlayerCubeState(player.displayName, true);
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "OnPlayerDeath");
        }

        if (respawnPoint != null)
        {
            player.TeleportTo(respawnPoint.position, respawnPoint.rotation);
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
        // Find and destroy the gun
        GameObject endPlatformObj = GameObject.Find("EndPlatform");
        if (endPlatformObj != null)
        {
            EndPlatform endPlatform = endPlatformObj.GetComponent<EndPlatform>();
            if (endPlatform != null)
            {
                endPlatform.DestroyGun();
                Debug.Log("[DyingFell] Gun destroyed globally");
            }
        }
    }
} 