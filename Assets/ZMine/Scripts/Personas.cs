using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

public class Personas : UdonSharpBehaviour
{
    public GameObject[] personaObjects; // Array of Persona objects

    private void Update()
    {
        int playerCount = VRCPlayerApi.GetPlayerCount();
        VRCPlayerApi[] players = new VRCPlayerApi[playerCount];
        VRCPlayerApi.GetPlayers(players);

        // Log the list of player names
        string playerNames = "Player Names: ";
        for (int i = 0; i < playerCount; i++)
        {
            playerNames += players[i].displayName + ", ";
        }
        Debug.Log(playerNames);

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
}