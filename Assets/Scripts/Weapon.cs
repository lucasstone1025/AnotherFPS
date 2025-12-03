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
    public float spreadIntensity;

    // Magic Ball (formerly Bullet)
    public GameObject magicBallPrefab;      // Assign your magic ball prefab here
    public Transform bulletSpawn;
    public float magicBallVelocity = 15f;   // Slower for better visibility
    public float magicBallLifeTime = 5f;    // How long before it despawns
    public float launchAngle = 10f;         // Upward angle for arc trajectory

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

        Vector3 shootingDirection = CalculateDirectionAndSpread().normalized;

        // Instantiate magic ball 
        GameObject magicBall = Instantiate(magicBallPrefab, bulletSpawn.position, Quaternion.identity);

        // Get the rigidbody
        Rigidbody rb = magicBall.GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            // Enable gravity for projectile motion
            rb.useGravity = true;
            
            // Calculate launch direction with upward angle for arc
            Vector3 launchDirection = shootingDirection;
            launchDirection.y += Mathf.Tan(launchAngle * Mathf.Deg2Rad);
            launchDirection.Normalize();
            
            // Apply velocity for projectile motion
            rb.linearVelocity = launchDirection * magicBallVelocity;
        }

        // Destroy magic ball after certain time
        StartCoroutine(DestroyBulletAfterTime(magicBall, magicBallLifeTime));

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

        Vector3 direction = targetPoint - bulletSpawn.position;

        float x = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
        float y = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);

        // return the shooting direction and spread 
        return direction + new Vector3(x, y, 0);

    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
    }
}