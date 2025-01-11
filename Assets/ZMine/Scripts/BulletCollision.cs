using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

public class BulletCollision : UdonSharpBehaviour
{
    private TeleportManager teleportManager;

    void Start()
    {
        GameObject managerObj = GameObject.Find("TeleportManager");
        if (managerObj != null)
        {
            teleportManager = managerObj.GetComponent<TeleportManager>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (teleportManager == null) 
        {
            Debug.LogError("[BulletCollision] TeleportManager not found!");
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
                teleportManager.TeleportPlayer(playerName);
            }
        }
    }
}