using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 40f;
    public float ragdollDestroyDelay = 5f;  // How long before ragdoll is removed
    public bool useRagdoll = true;           // Toggle ragdoll on/off

    private float currentHealth;
    private bool isDead = false;
    private Animator animator;
    private Rigidbody[] ragdollRigidbodies;

    private void Start()
    {
        currentHealth = maxHealth;

        // Get animator component
        animator = GetComponent<Animator>();

        // Find all rigidbodies (for ragdoll)
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();

        // Disable ragdoll rigidbodies at start (keep them kinematic)
        DisableRagdoll();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // Notify GameManager of the kill
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterKill();
        }

        // Change tag to prevent projectiles from hitting dead skeletons
        gameObject.tag = "Untagged";

        // Disable all trigger colliders so projectiles pass through dead bodies
        Collider[] allColliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in allColliders)
        {
            // Only disable trigger colliders (keep physics colliders for ragdoll)
            if (col.isTrigger)
            {
                col.enabled = false;
            }
        }

        if (useRagdoll)
        {
            ActivateRagdoll();
        }

        // Disable AI movement script if present
        AI_movement aiMovement = GetComponent<AI_movement>();
        if (aiMovement != null)
        {
            aiMovement.enabled = false;
        }

        // Disable this script
        this.enabled = false;

        // Destroy the skeleton after delay
        Destroy(gameObject, ragdollDestroyDelay);
    }

    void DisableRagdoll()
    {
        // Disable all ragdoll rigidbodies (make them kinematic)
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.isKinematic = true;
        }
    }

    void ActivateRagdoll()
    {
        // Disable animator to let physics take over
        if (animator != null)
        {
            animator.enabled = false;
        }

        // Enable all ragdoll rigidbodies
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.isKinematic = false;
        }

        // Add a bit of random force for dramatic effect (optional)
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            Vector3 randomForce = new Vector3(
                Random.Range(-100f, 100f),
                Random.Range(50f, 150f),
                Random.Range(-100f, 100f)
            );
            rb.AddForce(randomForce);
        }
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}
