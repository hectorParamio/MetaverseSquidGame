using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Components;

public class EndPlatform : UdonSharpBehaviour
{
    public GameObject gunPrefab;
    public ParticleSystem muzzleFlash;
    public AudioSource shootSound;
    private bool hasGun = false;
    private VRCPlayerApi localPlayer;
    private GameObject activeGun;
    private bool isVR = false;
    private VRCPickup gunPickup;

    void Start()
    {
        Debug.Log("[EndPlatform] Script started");
        localPlayer = Networking.LocalPlayer;
        isVR = localPlayer.IsUserInVR();
        
        if (gunPrefab != null)
        {
            Debug.Log("[EndPlatform] Attempting to create gun");
            activeGun = VRCInstantiate(gunPrefab);
            activeGun.transform.localScale = new Vector3(2f, 2f, 2f);
            activeGun.SetActive(false);
            gunPickup = activeGun.GetComponent<VRCPickup>();
            Debug.Log("[EndPlatform] Gun created");
        }
        else
        {
            Debug.LogError("[EndPlatform] Gun prefab is not assigned!");
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        Debug.Log("[EndPlatform] Trigger entered by player");
        if (player == localPlayer && !hasGun)
        {
            Debug.Log("[EndPlatform] Local player entered trigger");
            GiveGunToPlayer();
        }
    }

    void GiveGunToPlayer()
    {
        if (activeGun != null)
        {
            hasGun = true;
            activeGun.SetActive(true);
            
            // Position in front of the player's hand/view
            Vector3 spawnPosition;
            if (isVR)
            {
                // For VR, position near right hand
                spawnPosition = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position +
                              (localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).rotation * new Vector3(0, 0.1f, 0.1f));
            }
            else
            {
                // For desktop, position in front of head
                spawnPosition = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position + 
                              (localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation * new Vector3(0.3f, -0.1f, 0.5f));
            }
            
            activeGun.transform.position = spawnPosition;
            
            // Make sure the gun is pickupable
            if (gunPickup != null)
            {
                gunPickup.pickupable = true;
            }
            
            Debug.Log("[EndPlatform] Gun activated and positioned for pickup");
        }
        else
        {
            Debug.LogError("[EndPlatform] Active gun is null when trying to give to player!");
        }
    }
}