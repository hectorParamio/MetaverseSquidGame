using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

public class BulletCollision : UdonSharpBehaviour
{
    private TeleportManager teleportManager;
    private Personas personasManager;

    void Start()
    {
        GameObject managerObj = GameObject.Find("TeleportManager");
        if (managerObj != null)
        {
            teleportManager = managerObj.GetComponent<TeleportManager>();
        }

        GameObject personasObj = GameObject.Find("Personas");
        if (personasObj != null)
        {
            personasManager = personasObj.GetComponent<Personas>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (teleportManager == null || personasManager == null) 
        {
            Debug.LogError("[BulletCollision] Required managers not found!");
            return;
        }

        if (other.gameObject.name == "Cubo")
        {
            Transform parentTransform = other.transform.parent;
            TextMeshProUGUI textField = parentTransform.GetComponentInChildren<TextMeshProUGUI>();
            
            if (textField != null)
            {
                string playerName = textField.text;
                Debug.Log("[BulletCollision] Hit cube for player: " + playerName);
                personasManager.SetPlayerCubeState(playerName, true);
                teleportManager.TeleportPlayer(playerName);
            }
        }
    }
}