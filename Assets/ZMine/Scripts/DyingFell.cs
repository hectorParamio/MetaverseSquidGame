using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using TMPro;

public class DyingFell : UdonSharpBehaviour
{
    public Transform respawnPoint;
    [SerializeField] private Material deadMaterial;  // Assign this in inspector

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        Debug.Log($"[DyingFell] Player {player.displayName} fell, attempting to change cube state");
        
        // Navigate to Personas through the hierarchy
        Transform pantallaDeMuertos = transform.parent.Find("PantallaDeMuertos");
        if (pantallaDeMuertos != null)
        {
            Transform paredMuertos = pantallaDeMuertos.Find("ParedMuertos");
            if (paredMuertos != null)
            {
                Transform personas = paredMuertos.Find("Personas");
                if (personas != null)
                {
                    Debug.Log("[DyingFell] Found Personas object, searching for player's cube");
                    
                    // Now search through the Persona objects
                    foreach (Transform persona in personas)
                    {
                        TextMeshProUGUI textField = persona.GetComponentInChildren<TextMeshProUGUI>();
                        if (textField != null && textField.text == player.displayName)
                        {
                            Transform cubo = persona.Find("Cubo");
                            if (cubo != null)
                            {
                                Renderer cubeRenderer = cubo.GetComponent<Renderer>();
                                if (cubeRenderer != null)
                                {
                                    cubeRenderer.material = deadMaterial;
                                    Debug.Log($"[DyingFell] Changed material for {player.displayName}'s cube");
                                }
                            }
                            break;
                        }
                    }
                }
                else
                {
                    Debug.LogError("[DyingFell] Could not find Personas object!");
                }
            }
            else
            {
                Debug.LogError("[DyingFell] Could not find ParedMuertos object!");
            }
        }
        else
        {
            Debug.LogError("[DyingFell] Could not find PantallaDeMuertos object!");
        }

        player.TeleportTo(respawnPoint.position, respawnPoint.rotation);
    }
} 