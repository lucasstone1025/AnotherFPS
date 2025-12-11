using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Camera playerCamera;

    // Projectile settings
    public GameObject projectilePrefab;
    public Transform projectileSpawn;
    public float projectileVelocity = 50f;      // Faster projectile speed
    public float projectileLifeTime = 10f;      // Longer lifetime to reach distant targets
    public float projectileSpawnOffset = 1.0f;  // Small offset to avoid player collision

    // Cooldown settings
    public float shootCooldown = 0.25f;  // Time between shots in seconds (quarter second cooldown)
    private float nextFireTime = 0f;    // Time when we can fire next
    
    //audio
    private AudioSource audioSource;
    public AudioClip shootSound;

    void Update()
    {
        // Only shoot on mouse click (not hold) and if cooldown is ready
        if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time >= nextFireTime)
        {
            FireProjectile();
        }
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void FireProjectile()
    {
        // Validation checks
        if (projectilePrefab == null || projectileSpawn == null) return;

        // Set next fire time (cooldown)
        nextFireTime = Time.time + shootCooldown;

        // Get the exact target point from raycast
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        Vector3 targetPoint;
        int layerMask = ~LayerMask.GetMask("Player");

        if (Physics.Raycast(ray, out hit, 1000f, layerMask))
        {
            targetPoint = hit.point;
            Debug.Log("Aiming at: " + hit.collider.name + " at position: " + hit.point);
        }
        else
        {
            targetPoint = ray.GetPoint(1000);
        }

        // Calculate direction from camera to target first
        Vector3 shootingDirection = (targetPoint - playerCamera.transform.position).normalized;

        // Spawn projectile slightly in front of spawn point in the shooting direction
        Vector3 spawnPosition = projectileSpawn.position + shootingDirection * projectileSpawnOffset;
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

        // Debug visualization
        Debug.DrawRay(spawnPosition, shootingDirection * 100f, Color.red, 2f);
        Debug.Log("Shooting direction: " + shootingDirection + " from spawn: " + spawnPosition);

        // Set up projectile physics
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearVelocity = shootingDirection * projectileVelocity;
        }

        // Ignore collisions with player and weapon
        IgnorePlayerCollisions(projectile);

        // Destroy projectile after lifetime
        StartCoroutine(DestroyProjectileAfterTime(projectile, projectileLifeTime));

        if (audioSource != null && shootSound != null)
{
        audioSource.PlayOneShot(shootSound);
}
    }

    private Vector3 CalculateShootingDirection()
    {
        // Shoot from center of camera view
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        Vector3 targetPoint;
        // Use a more precise raycast that hits exactly what we're aiming at
        // Ignore the player layer to prevent self-hits
        int layerMask = ~LayerMask.GetMask("Player");

        if (Physics.Raycast(ray, out hit, 1000f, layerMask))
        {
            // Aim at exactly what we're looking at
            targetPoint = hit.point;

            // Debug: Show what we're aiming at
            Debug.DrawLine(playerCamera.transform.position, hit.point, Color.green, 0.5f);
        }
        else
        {
            // Aim far forward if nothing hit
            targetPoint = ray.GetPoint(100);
        }

        // Direction from spawn point to target
        return targetPoint - projectileSpawn.position;
    }

    private void IgnorePlayerCollisions(GameObject projectile)
    {
        Collider projCollider = projectile.GetComponent<Collider>();
        if (projCollider == null) return;

        // Ignore player colliders
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Collider[] playerColliders = player.GetComponentsInChildren<Collider>();
            foreach (Collider col in playerColliders)
            {
                Physics.IgnoreCollision(projCollider, col);
            }
        }

        // Ignore weapon colliders
        Collider[] weaponColliders = GetComponentsInChildren<Collider>();
        foreach (Collider weaponCol in weaponColliders)
        {
            if (weaponCol != null)
            {
                Physics.IgnoreCollision(projCollider, weaponCol);
            }
        }
    }

    private IEnumerator DestroyProjectileAfterTime(GameObject projectile, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (projectile != null)
        {
            PointsTracker.instance.MissedEnemy();
            Destroy(projectile);
        }
    }

    // Public method to get cooldown info (for UI)
    public float GetCooldownTimeRemaining()
    {
        float timeRemaining = nextFireTime - Time.time;
        return Mathf.Max(0f, timeRemaining);
    }

    public bool IsReadyToFire()
    {
        return Time.time >= nextFireTime;
    }
}