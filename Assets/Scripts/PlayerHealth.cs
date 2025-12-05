using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    //Health Settings
    public float maxHealth = 100f;
    public float currentHealth;

    //UI References
    public Image healthBarFill;

    //Damage Settings
    public float damageCooldown = 1f; // Time in seconds between taking damage
    private float lastDamageTime;

    // Singleton for easy access from AI
    public static PlayerHealth Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        currentHealth = maxHealth;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateHealthBar();
    }

    public void TakeDamage(float damage)
    {
        //check cooldown to prevent rapid damage
        if (Time.time - lastDamageTime < damageCooldown)
            return;
        
        lastDamageTime = Time.time;
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthBar();

        Debug.Log("Player took " + damage + " damage. Current health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        //dont heal if already dead
        if (currentHealth <= 0)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        UpdateHealthBar();

        Debug.Log("Player healed " + amount + ". Current health: " + currentHealth);
    }

    public void FullHeal()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
        Debug.Log("Player fully healed. Current health: " + currentHealth);
    }

    void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
    }

    void Die()
    {
        // Handle player death (e.g., reload scene, show game over screen)
        Debug.Log("Player has died.");

        //for now just reload scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public bool CanTakeDamage()
    {
        return Time.time - lastDamageTime >= damageCooldown;
    }

    public bool isAlive()
    {
        return currentHealth > 0;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
