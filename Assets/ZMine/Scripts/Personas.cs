using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

public class Personas : UdonSharpBehaviour
{
    public GameObject[] personaObjects; // Array of Persona objects
    public Transform spawnPoint; // Reference to the spawn point

    private void Update()
    {
        int playerCount = VRCPlayerApi.GetPlayerCount();
        VRCPlayerApi[] players = new VRCPlayerApi[playerCount];
        VRCPlayerApi.GetPlayers(players);


        // Update text fields with player names
        for (int i = 0; i < personaObjects.Length; i++)
        {
            TextMeshProUGUI textField = personaObjects[i].GetComponentInChildren<TextMeshProUGUI>();
            if (textField != null)
            {
                if (i < playerCount)
                {
                    VRCPlayerApi player = players[i];
                    textField.text = player.displayName;
                }
                else
                {
                    textField.text = "";
                }
            }
        }
    }

    public void TeleportPlayerToSpawnPoint(string playerName)
    {
        int playerCount = VRCPlayerApi.GetPlayerCount();
        VRCPlayerApi[] players = new VRCPlayerApi[playerCount];
        VRCPlayerApi.GetPlayers(players);

        foreach (VRCPlayerApi player in players)
        {
            if (player.displayName == playerName)
            {
                player.TeleportTo(spawnPoint.position, spawnPoint.rotation);
                break;
            }
        }
    }
}