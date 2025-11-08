using UnityEngine;
using TMPro; // Import the TextMeshPro namespace

public class AmmoManager : MonoBehaviour
{
    public int maxAmmo = 25; // Maximum ammo
    private int currentAmmo; // Current ammo
    public TextMeshProUGUI ammoText; // Reference to the TextMeshProUGUI element

    void Start()
    {
        // Initialize ammo
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }

    public void AddAmmo(int amount)
    {
        currentAmmo += amount;
        if (currentAmmo > maxAmmo)
        {
            currentAmmo = maxAmmo; // Ensure ammo doesn't exceed max limit
        }
        UpdateAmmoUI();
    }


public bool UseAmmo(int amount)
{
    if (currentAmmo >= amount)
    {
        currentAmmo -= amount;
        UpdateAmmoUI();
        return true; // Ammo was successfully used
    }
    else
    {
        Debug.Log("Not enough ammo!");
        return false; // Not enough ammo
    }
}

    private void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = "Ammo: " + currentAmmo;
        }
    }
}