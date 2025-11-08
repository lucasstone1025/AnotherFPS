// 11/7/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEditor;
using UnityEngine;

public class AddAmmoExample : MonoBehaviour
{
    public AmmoManager ammoManager;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) // Press 'R' to reload
        {
            ammoManager.AddAmmo(5); // Add 5 ammo
        }
    }
}
