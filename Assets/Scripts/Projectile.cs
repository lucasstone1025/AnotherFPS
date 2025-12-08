using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject explosionEffectPrefab;  // Assign a particle effect for explosion
    public float explosionRadius = 5f;        // How far the explosion affects
    public float explosionForce = 700f;       // Force applied to nearby objects
    public float damage = 100f;               // One-hit kill damage
    public float gravityScale = 0.3f;         // Reduced gravity for hovering effect
    
    // Trail effect to make it more visible
    private TrailRenderer trail;
    private bool hasExploded = false;
    private float spawnTime;
    private Rigidbody rb;

    void Start()
    {
        spawnTime = Time.time;
        
        // Get rigidbody and apply custom gravity
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false; // We'll apply custom gravity
            rb.mass = 0.5f;
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
        
        // Set collider to trigger mode for reliable detection
        Collider projectileCollider = GetComponent<Collider>();
        if (projectileCollider != null)
        {
            projectileCollider.isTrigger = true;
        }
        
        // Ignore collision with player and weapon
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Collider playerCollider = player.GetComponent<Collider>();
            Collider thisCollider = GetComponent<Collider>();
            
            if (playerCollider != null && thisCollider != null)
            {
                Physics.IgnoreCollision(thisCollider, playerCollider);
            }
            
            // Also ignore all child colliders (like weapon, camera, etc.)
            Collider[] playerColliders = player.GetComponentsInChildren<Collider>();
            foreach (Collider playerCol in playerColliders)
            {
                if (thisCollider != null)
                {
                    Physics.IgnoreCollision(thisCollider, playerCol);
                }
            }
        }
        
        // Add a trail renderer if not present to make the ball more visible
        trail = GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
            trail.time = 0.5f;
            trail.startWidth = 0.2f;
            trail.endWidth = 0.05f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = Color.red;
            trail.endColor = new Color(1f, 0.5f, 0f, 0f); // Orange fade
        }
    }

    void FixedUpdate()
    {
        // Apply custom reduced gravity
        if (rb != null && !hasExploded)
        {
            rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Ignore collisions in the first 0.1 seconds after spawn (safety buffer)
        if (Time.time - spawnTime < 0.1f) return;
        
        // Don't explode if hitting player or weapon
        if (collision.gameObject.CompareTag("Player")) return;
        if (collision.gameObject.CompareTag("Weapon")) return;
        
        if (hasExploded) return; // Prevent multiple explosions
        
        Debug.Log("Projectile collided with: " + collision.gameObject.name + " (Tag: " + collision.gameObject.tag + ")");
        
        // Explode on any impact
        Explode(collision.contacts[0].point);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Ignore triggers in the first 0.1 seconds after spawn
        if (Time.time - spawnTime < 0.1f) return;
        
        // Don't explode if hitting player or weapon
        if (other.CompareTag("Player")) return;
        if (other.CompareTag("Weapon")) return;
        
        if (hasExploded) return;
        
        Debug.Log("Projectile triggered by: " + other.gameObject.name + " (Tag: " + other.gameObject.tag + ")");
        
        // Explode on any trigger contact
        Explode(transform.position);
    }

    void Explode(Vector3 explosionPoint)
    {
        hasExploded = true;

        // Create explosion visual effect
        if (explosionEffectPrefab != null)
        {
            GameObject explosion = Instantiate(explosionEffectPrefab, explosionPoint, Quaternion.identity);
            Destroy(explosion, 3f); // Destroy effect after 3 seconds
        }
        else if (GlobalReferences.instance != null && GlobalReferences.instance.explosionEffectPrefab != null)
        {
            GameObject explosion = Instantiate(GlobalReferences.instance.explosionEffectPrefab, explosionPoint, Quaternion.identity);
            Destroy(explosion, 2f);
        }

        // Find all colliders in explosion radius (check everything)
        Collider[] colliders = Physics.OverlapSphere(explosionPoint, explosionRadius);

        Debug.Log($"Explosion at {explosionPoint}, found {colliders.Length} objects in radius");

        foreach (Collider nearbyObject in colliders)
        {
            // Skip the magic ball itself
            if (nearbyObject.gameObject == gameObject) continue;
            
            // Skip player
            if (nearbyObject.CompareTag("Player") || nearbyObject.transform.root.CompareTag("Player")) continue;

            float distance = Vector3.Distance(explosionPoint, nearbyObject.transform.position);
            Debug.Log($"Object in range: {nearbyObject.name}, Tag: {nearbyObject.tag}, Distance: {distance}");

            // Apply explosion force to objects with Rigidbody (but not player)
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null && !nearbyObject.CompareTag("Player"))
            {
                rb.AddExplosionForce(explosionForce, explosionPoint, explosionRadius);
            }

            // Damage enemies - check tag first, then name, then try parent
            bool isEnemy = nearbyObject.CompareTag("Enemy") || 
                          nearbyObject.name.ToLower().Contains("skeleton");
            
            GameObject targetObject = nearbyObject.gameObject;
            
            // Also check parent object
            if (!isEnemy && nearbyObject.transform.parent != null)
            {
                isEnemy = nearbyObject.transform.parent.CompareTag("Enemy") || 
                         nearbyObject.transform.parent.name.ToLower().Contains("skeleton");
                
                // If parent is enemy, target parent instead
                if (isEnemy)
                {
                    targetObject = nearbyObject.transform.parent.gameObject;
                }
            }

            if (isEnemy)
            {
                // Try to deal damage with health system
                EnemyHealth enemyHealth = targetObject.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);
                    Debug.Log("Dealt " + damage + " damage to enemy: " + targetObject.name);
                }
                else
                {
                    // Fallback: destroy if no health component
                    Destroy(targetObject);
                    Debug.Log("Destroyed enemy (no health): " + targetObject.name);
                }
                
                // Only damage one enemy per explosion
                break;
            }

            // Destroy targets
            if (nearbyObject.CompareTag("Target"))
            {
                Destroy(nearbyObject.gameObject);
                Debug.Log("Destroyed target: " + nearbyObject.name);
            }
        }

        // Destroy the projectile
        Destroy(gameObject);
    }

    // Optional: Visualize explosion radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
