using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class EndPlatform : UdonSharpBehaviour
{
    public GameObject gunPrefab;
    public ParticleSystem muzzleFlash;
    public AudioSource shootSound;
    private bool hasGun = false;
    private VRCPlayerApi localPlayer;
    private GameObject activeGun;
    private bool isVR = false;

    void Start()
    {
        Debug.Log("[EndPlatform] Script started");
        localPlayer = Networking.LocalPlayer;
        isVR = localPlayer.IsUserInVR();
        
        if (gunPrefab != null)
        {
            activeGun = VRCInstantiate(gunPrefab);
            activeGun.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            activeGun.SetActive(false);
            Debug.Log("[EndPlatform] Gun created");
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
            Debug.Log("[EndPlatform] Gun activated");
        }
    }

    void Update()
    {
        if (hasGun && activeGun != null)
        {
            UpdateGunPosition();
        }
    }

    void UpdateGunPosition()
    {
        if (isVR)
        {
            // VR hand tracking
            Vector3 handPosition = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
            Quaternion handRotation = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).rotation;
            
            activeGun.transform.position = handPosition;
            activeGun.transform.rotation = handRotation * Quaternion.Euler(0, 90, 0);
        }
        else
        {
            // Desktop mode - follow camera
            Vector3 playerPosition = localPlayer.GetPosition();
            Quaternion playerRotation = localPlayer.GetRotation();
            
            Vector3 offset = playerRotation * new Vector3(0.3f, 1.2f, 0.5f);
            activeGun.transform.position = playerPosition + offset;
            activeGun.transform.rotation = playerRotation * Quaternion.Euler(0, 90, 0);
        }
    }
}