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

    //audio
    public AudioClip hurtSound;
    private AudioSource audioSource;

    void Awake()
    {
        Instance = this;
        currentHealth = maxHealth;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateHealthBar();
        audioSource = GetComponent<AudioSource>();

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
        // Play hurt sound
        if (hurtSound != null && audioSource != null && currentHealth > 0)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f); // optional variation
            audioSource.PlayOneShot(hurtSound);
        }

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
    }

    public void FullHeal()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
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
        // Notify GameManager of player death
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
        else
        {
            // Fallback if no GameManager (for testing)
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
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
