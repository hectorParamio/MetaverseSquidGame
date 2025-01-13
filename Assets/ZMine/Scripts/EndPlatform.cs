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
    [Header("Audio")]
    public AudioSource shootSound;
    public AudioClip shootClip;
    public AudioSource nobulletSound;
    public AudioClip nobulletClip;
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
    private readonly float[] failureProbabilities = { 0f, 00f, 0f }; // Failure chances for each shot (reverse order)
    [UdonSynced] private bool isGunActive = false;
    private VRCObjectSync gunObjectSync;
    [Header("Timer Settings")]
    public CountdownTimer countdownTimer;
    public AudioSource triggerSound;
    public AudioClip triggerSoundClip;
    private Personas personasManager;
    public DoorController doorController;
    [UdonSynced] private bool triggerSoundHasPlayed = false;
    private bool previousTriggerSoundState = false;

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

        GameObject personasObj = GameObject.Find("Personas");
        if (personasObj != null)
        {
            personasManager = personasObj.GetComponent<Personas>();
        }
    }




    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        Debug.Log($"[EndPlatform] Trigger entered by player: {player.displayName}");
        
        // Play trigger sound only once globally, regardless of who triggers it
        if (!triggerSoundHasPlayed)
        {
            // Take ownership to sync the state
            Networking.SetOwner(player, gameObject);
            triggerSoundHasPlayed = true;
            if (triggerSound != null && triggerSoundClip != null)
            {
                triggerSound.PlayOneShot(triggerSoundClip);
            }
            RequestSerialization();
        }

        if (doorController != null && doorController.song != null && doorController.song.isPlaying)
        {
            doorController.song.Stop();
            Debug.Log("[EndPlatform] Song stopped successfully.");
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
            RequestSerialization();
            activeGun.SetActive(true);
            
            // Unified positioning for all players
            Vector3 spawnPosition = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position +
                                 (localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation * gunOffset);
            activeGun.transform.position = spawnPosition;
            isHeld = true; // Auto-pickup for all users
            
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
        if (hasGun)
        {
            // Handle gun pickup toggle
            if (Input.GetKeyDown(KeyCode.F))
            {
                isHeld = !isHeld;
                Debug.Log($"[EndPlatform] Gun held: {isHeld}");
            }

            if (isHeld)
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
            nobulletSound.PlayOneShot(nobulletClip);
            return;
        }

        // Play sound effect
        if (shootSound != null && shootClip != null)
        {
            Debug.Log("[EndPlatform] Playing shoot sound");
            shootSound.PlayOneShot(shootClip);
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

                if (personasManager != null)
                {
                    personasManager.SetPlayerCubeState(localPlayer.displayName, true);
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "OnPlayerDeath");
                }

                DestroyGun();
                Debug.Log("[EndPlatform] Shot backfired! Bullet aimed at the player.");

            }
            else
            {
                // Aim forward
                shootDirection = (localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation * Vector3.forward).normalized;
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
        if (triggerSoundHasPlayed && !previousTriggerSoundState)
        {
            if (triggerSound != null && triggerSoundClip != null)
            {
                triggerSound.PlayOneShot(triggerSoundClip);
            }
            previousTriggerSoundState = true;
        }
        
        if (activeGun != null)
        {
            activeGun.SetActive(isGunActive);
        }
    }

    public void SyncPlayerDeath(string playerName)
    {
        if (personasManager != null)
        {
            personasManager.SetPlayerCubeState(playerName, true);
            
            // If this is the local player who died, destroy their gun
            if (localPlayer != null && playerName == localPlayer.displayName)
            {
                OnPlayerDeath();
            }
        }
    }

    public void DestroyGun()
    {
        if (activeGun != null)
        {
            // Take ownership before modifying networked state
            if (!Networking.IsOwner(activeGun))
            {
                Networking.SetOwner(localPlayer, activeGun);
            }
            
            hasGun = false;
            isGunActive = false;
            isHeld = false;
            RequestSerialization();
            activeGun.SetActive(false);
            Debug.Log("[EndPlatform] Gun deactivated due to player death");
        }
    }

    // Add this new method to handle all death scenarios
    public void OnPlayerDeath()
    {
        if (Networking.LocalPlayer == localPlayer && hasGun)
        {
            DestroyGun();
        }
    }





}