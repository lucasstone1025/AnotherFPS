using System;
using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{

    public Camera playerCamera;

    //shooting
    public bool isShooting, readyToShoot;
    bool allowReset = true;
    public float shootingDelay = 0.2f;

    //burst 
    public int bulletsPerBurst = 3;
    public int burstBulletsLeft;

    //spread
    public float spreadIntensity = 0f;

    // Projectile
    public GameObject projectilePrefab;
    public Transform projectileSpawn;
    public float projectileVelocity = 15f; // Slower velocity for visible arc
    public float projectileLifeTime = 5f;
    public float launchAngle = 15f; // Angle above horizontal for arc

    public enum ShootingMode
    {
        Single,
        Burst,
        Auto
    }

    public ShootingMode currentShootingMode;

    private void Awake()
    {
        readyToShoot = true;
        burstBulletsLeft = bulletsPerBurst;
    }
    
    void Update()
    {
        if (currentShootingMode == ShootingMode.Auto)
        {
            //hold to shoot
            isShooting = Input.GetKey(KeyCode.Mouse0);
        }
        else if (currentShootingMode == ShootingMode.Single || currentShootingMode == ShootingMode.Burst)
        {
            //press to shoot
            isShooting = Input.GetKeyDown(KeyCode.Mouse0);
        }
        
        if (readyToShoot && isShooting)
        {
            burstBulletsLeft = bulletsPerBurst;
            FireWeapon();
        }
    }

    private void FireWeapon()
    {
        readyToShoot = false;

        // Debug checks
        if (projectilePrefab == null)
        {
            Debug.LogError("Projectile Prefab is not assigned!");
            return;
        }
        if (projectileSpawn == null)
        {
            Debug.LogError("Projectile Spawn is not assigned!");
            return;
        }

        Debug.Log("Firing projectile at position: " + projectileSpawn.position);

        Vector3 shootingDirection = CalculateDirectionAndSpread().normalized;

        // Spawn projectile forward along the shooting direction (not spawn transform's forward)
        Vector3 spawnPosition = projectileSpawn.position + shootingDirection * 1.5f;
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

        // Add trail renderer for red trail effect
        TrailRenderer trail = projectile.GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = projectile.AddComponent<TrailRenderer>();
            trail.time = 0.5f;
            trail.startWidth = 0.2f;
            trail.endWidth = 0.05f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = Color.red;
            trail.endColor = new Color(1f, 0.5f, 0f, 0f); // Orange fade
        }

        // Calculate launch velocity - shoot straight forward
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.useGravity = false; // Projectile script handles custom gravity
        
        // Ignore all player colliders
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Collider projectileCollider = projectile.GetComponent<Collider>();
            Collider[] playerColliders = player.GetComponentsInChildren<Collider>();
            foreach (Collider col in playerColliders)
            {
                Physics.IgnoreCollision(projectileCollider, col);
            }
        }
        
        // Use the shooting direction directly for straight flight
        rb.linearVelocity = shootingDirection * projectileVelocity;

        //destroy projectile after certain time
        StartCoroutine(DestroyProjectileAfterTime(projectile, projectileLifeTime));

        // check if done shooting
        if (allowReset)
        {
            //reset shot after delay
            Invoke("ResetShot", shootingDelay);
            allowReset = false;
        }

        //burst mode
        if (currentShootingMode == ShootingMode.Burst && burstBulletsLeft > 1)
        {
            burstBulletsLeft--;
            Invoke("FireWeapon", shootingDelay); // small delay between burst shots
        }


    }
    
    private void ResetShot()
    {
        readyToShoot = true;
        allowReset = true;
    }

    public Vector3 CalculateDirectionAndSpread()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100); // some far away point
        }

        Vector3 direction = targetPoint - projectileSpawn.position;

        // Apply spread in camera space, not world space
        if (spreadIntensity > 0)
        {
            float x = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
            float y = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
            
            direction += playerCamera.transform.right * x;
            direction += playerCamera.transform.up * y;
        }

        // return the shooting direction with spread
        return direction;

    }

    private IEnumerator DestroyProjectileAfterTime(GameObject projectile, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (projectile != null)
        {
            Destroy(projectile);
        }
    }
}