using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponCooldownUI : MonoBehaviour
{
    [Header("References")]
    public Weapon weapon;  // Reference to the weapon script

    [Header("UI Elements")]
    public Image cooldownFillImage;  // Circular fill image (optional)
    public TextMeshProUGUI cooldownText;  // Text showing seconds (optional)

    void Update()
    {
        if (weapon == null) return;

        float cooldownRemaining = weapon.GetCooldownTimeRemaining();
        bool isReady = weapon.IsReadyToFire();

        // Update fill image if assigned
        if (cooldownFillImage != null)
        {
            if (isReady)
            {
                // Weapon is ready - show full (green)
                cooldownFillImage.fillAmount = 1f;
                cooldownFillImage.color = Color.green;
            }
            else
            {
                // Weapon is on cooldown - show progress (red to green)
                float cooldownProgress = 1f - (cooldownRemaining / weapon.shootCooldown);
                cooldownFillImage.fillAmount = cooldownProgress;
                cooldownFillImage.color = Color.Lerp(Color.red, Color.green, cooldownProgress);
            }
        }

        // Update text if assigned
        if (cooldownText != null)
        {
            if (isReady)
            {
                cooldownText.text = "READY";
                cooldownText.color = Color.green;
            }
            else
            {
                cooldownText.text = cooldownRemaining.ToString("F1") + "s";
                cooldownText.color = Color.red;
            }
        }
    }
}
