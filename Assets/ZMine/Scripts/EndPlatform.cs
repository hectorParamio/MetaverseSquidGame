using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.SDK3.Components;

public class EndPlatform : UdonSharpBehaviour
{
    public GameObject gunPrefab;
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float bulletSpeed = 60f;
    public float bulletLifetime = 2f;
    public ParticleSystem muzzleFlash;
    [Header("Audio")]
    public AudioSource shootSound;
    public AudioClip shootClip;
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
    private Vector3 gunOffset = new Vector3(0.5f, -0.3f, 0.7f);
    public Transform respawnPoint;
    // NEW: Variables for shot mechanics
    private int shotsFired = 0; // Track shots fired
    private const int maxShots = 3; // Maximum shots allowed
    private readonly float[] failureProbabilities = { 10f, 40f, 60f }; // Failure chances for each shot (reverse order)
    [UdonSynced] private bool isGunActive = false;
    private VRCObjectSync gunObjectSync;
    [Header("Timer Settings")]
    public CountdownTimer countdownTimer;
    public AudioSource triggerSound;
    public AudioClip triggerSoundClip;

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
            gunObjectSync = activeGun.GetComponent<VRCObjectSync>();
            if (gunObjectSync == null)
            {
                Debug.LogError("[EndPlatform] VRCObjectSync component missing on gun prefab!");
            }
            Debug.Log($"[EndPlatform] Gun instantiated: {activeGun != null}");
            activeGun.transform.localScale = new Vector3(2f, 2f, 2f);
            activeGun.SetActive(false);

            bulletSpawnPoint = activeGun.transform.Find("Bullet Spawn Point");
            if (bulletSpawnPoint == null)
            {
                Debug.LogError("[EndPlatform] Gun prefab is not assigned!");
                Debug.LogError("[EndPlatform] Bullet Spawn Point not found in gun prefab! Falling back to gun transform.");
                bulletSpawnPoint = activeGun.transform;
            }

            gunPickup = activeGun.GetComponent<VRCPickup>();
            Debug.Log($"[EndPlatform] Gun pickup component found: {gunPickup != null}");
        }
        else
        {
            Debug.LogError("[EndPlatform] Gun prefab is not assigned!");
        }

        if (shootSound != null)
        {
            shootSound.playOnAwake = false;
            if (shootClip != null)
            {
                shootSound.clip = shootClip;
            }
        }
    }




    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        Debug.Log($"[EndPlatform] Trigger entered by player: {player.displayName}");
        
        // Play trigger sound
        if (triggerSound != null && triggerSoundClip != null)
        {
            triggerSound.PlayOneShot(triggerSoundClip);
        }

        // Set countdown timer to 13 if it's higher
        if (countdownTimer != null)
        {
            float currentTime = countdownTimer.timeRemaining;
            if (currentTime > 12f)
            {
                countdownTimer.SetTime(12f);
            }
        }

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
            Networking.SetOwner(localPlayer, activeGun);
            hasGun = true;
            isGunActive = true;
            RequestSerialization(); // Sync the gun state
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
        // TryShoot();
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

        if (shotsFired >= maxShots)
        {
            Debug.LogWarning("[EndPlatform] This gun is out of bullets!");
            return;
        }

        // Play sound effect
        if (shootSound != null && shootClip != null)
        {
            Debug.Log("[EndPlatform] Playing shoot sound");
            shootSound.PlayOneShot(shootClip);
        }
        else
        {
            Debug.LogWarning("[EndPlatform] No shoot sound or audio clip assigned!");
        }

        // Play muzzle flash
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        // Determine if the shot should backfire
        bool backfires = Random.Range(0f, 100f) < failureProbabilities[shotsFired];

        // Spawn bullet
        if (bulletPrefab != null)
        {
            GameObject bullet = VRCInstantiate(bulletPrefab);

            Vector3 spawnPos = bulletSpawnPoint != null
                ? bulletSpawnPoint.position
                : activeGun.transform.position;

            bullet.transform.position = spawnPos;

            Vector3 shootDirection;
            if (backfires)
            {
                // Aim towards the player's face
                Vector3 playerHeadPosition = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
                shootDirection = (playerHeadPosition - spawnPos).normalized;
                localPlayer.TeleportTo(respawnPoint.position, respawnPoint.rotation);

                Debug.Log("[EndPlatform] Shot backfired! Bullet aimed at the player.");
            }
            else
            {
                // Aim forward
                if (isVR && gunPickup.IsHeld)
                {
                    Quaternion rightHandRotation = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).rotation;
                    shootDirection = (rightHandRotation * Vector3.forward).normalized;
                }
                else
                {
                    Quaternion cameraRotation = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
                    shootDirection = (cameraRotation * Vector3.forward).normalized;
                }
            }

            bullet.transform.rotation = Quaternion.LookRotation(shootDirection);
            bullet.transform.Rotate(90, 0, 0);

            // Apply force to the bullet
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            if (bulletRb != null)
            {
                bulletRb.velocity = shootDirection * bulletSpeed;
                Debug.Log($"[EndPlatform] Bullet velocity applied: {bulletRb.velocity}");
            }
            else
            {
                Debug.LogError("[EndPlatform] Bullet prefab is missing a Rigidbody component!");
            }

            Destroy(bullet, bulletLifetime);
        }

        // Increment shots fired
        shotsFired++;
        lastShootTime = Time.time;

        Debug.Log($"[EndPlatform] Shots fired: {shotsFired}/{maxShots}");

        // Handle self-inflicted death
        if (backfires)
        {
            Debug.LogError("[EndPlatform] Player is hit by their own bullet!");
            // Add self-death logic here (if needed)
        }
    }

    public override void OnDeserialization()
    {
        if (activeGun != null)
        {
            activeGun.SetActive(isGunActive);
        }
    }





}