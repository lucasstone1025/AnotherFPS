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
    public float projectileSpawnOffset = 3.5f;  // Spawn farther forward to avoid collisions

    // Cooldown settings
    public float shootCooldown = 0.25f;  // Time between shots in seconds (quarter second cooldown)
    private float nextFireTime = 0f;    // Time when we can fire next

    void Update()
    {
        // Only shoot on mouse click (not hold) and if cooldown is ready
        if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time >= nextFireTime)
        {
            FireProjectile();
        }
    }

    private void FireProjectile()
    {
        // Validation checks
        if (projectilePrefab == null || projectileSpawn == null) return;

        // Set next fire time (cooldown)
        nextFireTime = Time.time + shootCooldown;

        // Calculate shooting direction from camera center
        Vector3 shootingDirection = CalculateShootingDirection().normalized;

        // Spawn projectile forward from spawn point
        Vector3 spawnPosition = projectileSpawn.position + shootingDirection * projectileSpawnOffset;
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

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
    }

    private Vector3 CalculateShootingDirection()
    {
        // Shoot from center of camera view
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
        {
            // Aim at what we're looking at
            targetPoint = hit.point;
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