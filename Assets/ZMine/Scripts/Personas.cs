using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

public class Personas : UdonSharpBehaviour
{
    public GameObject[] personaObjects; // Array of Persona objects
    public Transform spawnPoint; // Reference to the spawn point
    [SerializeField] private Material activeMaterial;   // Material for cubes with players
    [SerializeField] private Material inactiveMaterial; // Material for empty cubes
    [SerializeField] private Material deadMaterial;     // Material for dead/malfunctioned players
    [UdonSynced] private string[] deadPlayers = new string[16]; // Adjust size based on max players
    private int deadPlayerCount = 0;

    private void Update()
    {
        int playerCount = VRCPlayerApi.GetPlayerCount();
        VRCPlayerApi[] players = new VRCPlayerApi[playerCount];
        VRCPlayerApi.GetPlayers(players);

        // Debug.Log($"[Personas] Updating {personaObjects.Length} personas for {playerCount} players");

        // Update text fields and materials for each persona object
        for (int i = 0; i < personaObjects.Length; i++)
        {
            // Get the Text (TMP) component
            TextMeshProUGUI textField = personaObjects[i].GetComponentInChildren<TextMeshProUGUI>();
            if (textField == null)
            {
                // Debug.LogError($"[Personas] Persona {i}: Missing TextMeshProUGUI component");
                continue;
            }

            // Get Persona parent (same as BulletCollision but from Text)
            Transform personaTransform = textField.transform.parent.parent;
            Transform cuboTransform = personaTransform.Find("Cubo");
            
            // Debug.Log($"[Personas] Persona {i}: Looking for Cubo in {personaTransform.name}");
            
            if (cuboTransform == null)
            {
                // Debug.LogError($"[Personas] Persona {i}: Cannot find Cubo in {personaTransform.name}");
                continue;
            }

            Renderer cubeRenderer = cuboTransform.GetComponent<Renderer>();
            if (cubeRenderer == null)
            {
                // Debug.LogError($"[Personas] Persona {i}: Cubo missing Renderer component");
                continue;
            }

            if (i < playerCount)
            {
                VRCPlayerApi player = players[i];
                textField.text = player.displayName;
                
                // Check if the player is dead and set the appropriate material
                if (IsPlayerDead(player))
                {
                    cubeRenderer.material = deadMaterial;
                    // Debug.Log($"[Personas] Persona {i}: Set dead material for player {player.displayName}");
                }
                else
                {
                    cubeRenderer.material = activeMaterial;
                    // Debug.Log($"[Personas] Persona {i}: Set active material for player {player.displayName}");
                }
            }
            else
            {
                textField.text = "";
                cubeRenderer.material = inactiveMaterial;
                // Debug.Log($"[Personas] Persona {i}: Set inactive material (no player)");
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

    public void SetPlayerCubeState(string playerName, bool isDead)
    {
        // Sincroniza la lista de jugadores muertos
        if (isDead)
        {
            // Si el jugador no está ya en la lista de muertos, lo añade
            bool alreadyDead = false;
            for (int i = 0; i < deadPlayerCount; i++)
            {
                if (deadPlayers[i] == playerName)
                {
                    alreadyDead = true;
                    break;
                }
            }

            if (!alreadyDead && deadPlayerCount < deadPlayers.Length)
            {
                deadPlayers[deadPlayerCount] = playerName;
                deadPlayerCount++;
                RequestSerialization(); // Sincroniza la lista de jugadores muertos
            }
        }

        // Cambia el estado visual del cubo
        foreach (GameObject personaObj in personaObjects)
        {
            TextMeshProUGUI textField = personaObj.GetComponentInChildren<TextMeshProUGUI>();
            if (textField != null && textField.text == playerName)
            {
                Transform cuboTransform = personaObj.transform.Find("Cubo");
                if (cuboTransform != null)
                {
                    Renderer cubeRenderer = cuboTransform.GetComponent<Renderer>();
                    if (cubeRenderer != null)
                    {
                        cubeRenderer.material = isDead ? deadMaterial : activeMaterial;
                    }
                }
                break;
            }
        }
    }

    public override void OnDeserialization()
    {
        // Update all dead players' cubes
        for (int i = 0; i < deadPlayerCount; i++)
        {
            if (!string.IsNullOrEmpty(deadPlayers[i]))
            {
                SetPlayerCubeState(deadPlayers[i], true);
            }
        }
    }

    private bool IsPlayerDead(VRCPlayerApi player)
    {
        for (int i = 0; i < deadPlayerCount; i++)
        {
            if (deadPlayers[i] == player.displayName)
            {
                return true;
            }
        }
        return false;
    }

    public void SyncPlayerDeath(string playerName)
    {
        SetPlayerCubeState(playerName, true);
    }
}