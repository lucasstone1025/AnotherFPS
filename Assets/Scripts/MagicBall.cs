using UnityEngine;

public class MagicBall : MonoBehaviour
{
    public GameObject explosionEffectPrefab;  // Assign a particle effect for explosion
    public float explosionRadius = 5f;        // How far the explosion affects
    public float explosionForce = 700f;       // Force applied to nearby objects
    public float damage = 50f;                // Damage dealt to enemies
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
            foreach (Collider col in playerColliders)
            {
                if (thisCollider != null)
                {
                    Physics.IgnoreCollision(thisCollider, col);
                }
            }
        }
        
        // Add a trail renderer if not present to make the ball more visible
        trail = GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
            trail.time = 0.5f;
            trail.startWidth = 0.3f;
            trail.endWidth = 0.05f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = Color.cyan;
            trail.endColor = new Color(0, 1, 1, 0);
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
        
        if (hasExploded) return; // Prevent multiple explosions
        
        // Explode on any impact
        Explode(collision.contacts[0].point);
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

        // Find all colliders in explosion radius (check everything)
        Collider[] colliders = Physics.OverlapSphere(explosionPoint, explosionRadius);

        Debug.Log($"Explosion at {explosionPoint}, found {colliders.Length} objects in radius");

        foreach (Collider nearbyObject in colliders)
        {
            // Skip the magic ball itself
            if (nearbyObject.gameObject == gameObject) continue;

            float distance = Vector3.Distance(explosionPoint, nearbyObject.transform.position);
            Debug.Log($"Object in range: {nearbyObject.name}, Tag: {nearbyObject.tag}, Distance: {distance}");

            // Apply explosion force to objects with Rigidbody
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, explosionPoint, explosionRadius);
            }

            // Damage enemies - check tag first, then name, then try parent
            bool isEnemy = nearbyObject.CompareTag("Enemy") || 
                          nearbyObject.name.ToLower().Contains("skeleton");
            
            // Also check parent object
            if (!isEnemy && nearbyObject.transform.parent != null)
            {
                isEnemy = nearbyObject.transform.parent.CompareTag("Enemy") || 
                         nearbyObject.transform.parent.name.ToLower().Contains("skeleton");
                
                // If parent is enemy, destroy parent instead
                if (isEnemy)
                {
                    Destroy(nearbyObject.transform.parent.gameObject);
                    Debug.Log("Destroyed enemy (parent): " + nearbyObject.transform.parent.name);
                    continue;
                }
            }

            if (isEnemy)
            {
                Destroy(nearbyObject.gameObject);
                Debug.Log("Destroyed enemy: " + nearbyObject.name);
            }

            // Destroy targets
            if (nearbyObject.CompareTag("Target"))
            {
                Destroy(nearbyObject.gameObject);
                Debug.Log("Destroyed target: " + nearbyObject.name);
            }
        }

        // Destroy the magic ball
        Destroy(gameObject);
    }

    // Optional: Visualize explosion radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
