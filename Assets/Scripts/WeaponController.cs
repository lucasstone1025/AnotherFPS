// 11/7/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public AmmoManager ammoManager; // Reference to the AmmoManager script
    public GameObject bulletPrefab; // Bullet prefab
    public Transform firePoint; // Point from where bullets are fired
    public float bulletSpeed = 20f; // Speed of the bullet

    void Update()
    {
        // Check if the player presses the fire button
        if (Input.GetButtonDown("Fire1"))
        {
            FireWeapon();
        }
    }

    void FireWeapon()
    {
        if (ammoManager != null && ammoManager.UseAmmo(1)) // Use 1 ammo per shot
        {
            // Instantiate the bullet at the fire point
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.velocity = firePoint.forward * bulletSpeed; // Corrected property to set bullet speed
        }
        else
        {
            Debug.Log("Out of ammo!");
        }
    }
}
