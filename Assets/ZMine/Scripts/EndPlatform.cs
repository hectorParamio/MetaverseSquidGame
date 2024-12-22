using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Components;

public class EndPlatform : UdonSharpBehaviour
{
    public GameObject gunPrefab;
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float bulletSpeed = 30f;
    public float bulletLifetime = 3f;
    public ParticleSystem muzzleFlash;
    public AudioSource shootSound;
    private bool hasGun = false;
    private VRCPlayerApi localPlayer;
    private GameObject activeGun;
    private bool isVR = false;
    private VRCPickup gunPickup;
    [Header("Gun Settings")]
    public float shootForce = 20f;
    public float shootCooldown = 0.2f;
    private float lastShootTime;
    private bool isHeld = false;
    private bool isHoldingGun = false;
    private Vector3 gunOffset = new Vector3(0.5f, -0.3f, 0.7f); // Adjust these values to position the gun

    void Start()
    {
        Debug.Log("[EndPlatform] Script started");
        localPlayer = Networking.LocalPlayer;
        isVR = localPlayer.IsUserInVR();
        Debug.Log($"[EndPlatform] Is VR: {isVR}");
        
        if (gunPrefab != null)
        {
            Debug.Log("[EndPlatform] Attempting to create gun");
            activeGun = VRCInstantiate(gunPrefab);
            Debug.Log($"[EndPlatform] Gun instantiated: {activeGun != null}");
            activeGun.transform.localScale = new Vector3(2f, 2f, 2f);
            activeGun.SetActive(false);
            gunPickup = activeGun.GetComponent<VRCPickup>();
            Debug.Log($"[EndPlatform] Gun pickup component found: {gunPickup != null}");
        }
        else
        {
            Debug.LogError("[EndPlatform] Gun prefab is not assigned!");
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        Debug.Log($"[EndPlatform] Trigger entered by player: {player.displayName}");
        Debug.Log($"[EndPlatform] Is local player: {player == localPlayer}, Has gun: {hasGun}");
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
            
            if (isVR)
            {
                // VR positioning (unchanged)
                Vector3 spawnPosition = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position +
                                     (localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).rotation * new Vector3(0, 0.1f, 0.1f));
                activeGun.transform.position = spawnPosition;
            }
            else
            {
                // Desktop positioning
                Vector3 spawnPosition = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position +
                                     (localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation * gunOffset);
                activeGun.transform.position = spawnPosition;
                isHoldingGun = true; // Auto-pickup for desktop users
            }
            
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

    public override void OnPickup()
    {
        isHeld = true;
    }

    public override void OnDrop()
    {
        isHeld = false;
    }

    public override void OnPickupUseDown()
    {
        TryShoot();
    }

    public void Update()
    {
        if (!isVR && hasGun)
        {
            // Handle gun pickup with E key
            if (Input.GetKeyDown(KeyCode.E))
            {
                isHoldingGun = !isHoldingGun;
                Debug.Log($"[EndPlatform] Gun held: {isHoldingGun}");
            }

            if (isHoldingGun)
            {
                // Update gun position to follow player view
                Vector3 newPosition = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position +
                                   (localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation * gunOffset);
                
                Quaternion newRotation = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
                
                activeGun.transform.position = newPosition;
                activeGun.transform.rotation = newRotation;

                // Handle shooting with left mouse button
                if (Input.GetMouseButtonDown(0))
                {
                    TryShoot();
                }
            }
        }
    }

    private void TryShoot()
    {
        if (Time.time - lastShootTime < shootCooldown) return;
        
        // Play sound effect
        if (shootSound != null)
        {
            shootSound.PlayOneShot(shootSound.clip);
        }

        // Play muzzle flash
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        // Spawn bullet
        if (bulletPrefab != null && bulletSpawnPoint != null)
        {
            GameObject bullet = VRCInstantiate(bulletPrefab);
            
            // If no spawn point set, use gun's position
            Vector3 spawnPos = bulletSpawnPoint != null ? 
                bulletSpawnPoint.position : 
                activeGun.transform.position + activeGun.transform.forward * 0.5f;
            
            bullet.transform.position = spawnPos;
            
            // Set bullet direction based on VR/Desktop
            Vector3 shootDirection;
            if (isVR && gunPickup.IsHeld)
            {
                shootDirection = activeGun.transform.forward;
                bullet.transform.rotation = activeGun.transform.rotation;
            }
            else if (isHoldingGun)
            {
                shootDirection = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation * Vector3.forward;
                bullet.transform.rotation = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
            }
            else
            {
                return; // Don't shoot if not holding gun
            }

            // Add force to bullet
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            if (bulletRb != null)
            {
                bulletRb.velocity = shootDirection * bulletSpeed;
            }

            // Destroy bullet after lifetime
            Destroy(bullet, bulletLifetime);
        }

        lastShootTime = Time.time;
    }
}