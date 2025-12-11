using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject explosionEffectPrefab;  // Assign a particle effect for explosion
    public float explosionRadius = 7f;        // How far the explosion affects (increased for reliability)
    public float explosionForce = 700f;       // Force applied to nearby objects
    public float damage = 1000f;              // One-shot kill damage (very high to ensure kill)
    public float gravityScale = 0.05f;        // Very low gravity for nearly straight flight

    // Visual enhancements
    public Vector3 rotationSpeed = new Vector3(0, 360f, 0);  // Rotation per second
    public bool enablePulsing = true;         // Enable scale pulsing
    public float pulseSpeed = 3f;             // How fast it pulses
    public float pulseAmount = 0.2f;          // How much it scales (0.2 = 20%)
    public Color lightColor = new Color(1f, 0.5f, 0f);  // Orange glow
    public float lightIntensity = 3f;
    public float lightRange = 8f;

    // Trail effect to make it more visible
    private TrailRenderer trail;
    private Light projectileLight;
    private Vector3 originalScale;
    private bool hasExploded = false;
    private float spawnTime;
    private Rigidbody rb;

    void Start()
    {
        spawnTime = Time.time;
        originalScale = transform.localScale;

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

        // Use trigger mode for projectile
        Collider projectileCollider = GetComponent<Collider>();
        if (projectileCollider != null)
        {
            projectileCollider.isTrigger = true;  // Use trigger to pass through terrain
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

        // Set up Point Light for glow effect
        projectileLight = GetComponent<Light>();
        if (projectileLight == null)
        {
            projectileLight = gameObject.AddComponent<Light>();
        }
        projectileLight.type = LightType.Point;
        projectileLight.color = lightColor;
        projectileLight.intensity = lightIntensity;
        projectileLight.range = lightRange;
        projectileLight.shadows = LightShadows.None; // No shadows for performance

        // Improve trail renderer to make the ball more visible
        trail = GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
            trail.time = 0.7f;  // Longer trail
            trail.startWidth = 0.3f;
            trail.endWidth = 0.05f;

            // Try to find a working shader, fallback to Sprites/Default if Particles/Additive doesn't exist
            Shader trailShader = Shader.Find("Particles/Additive");
            if (trailShader == null)
            {
                trailShader = Shader.Find("Sprites/Default");
                // Silently fallback - no warning needed
            }

            if (trailShader != null)
            {
                Material trailMat = new Material(trailShader);
                trail.material = trailMat;
            }

            // Create gradient for better color
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(1f, 0.6f, 0f), 0.0f),  // Bright orange
                    new GradientColorKey(new Color(1f, 0.3f, 0f), 0.5f),  // Red-orange
                    new GradientColorKey(Color.red, 1.0f)                 // Red
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1.0f, 0.0f),
                    new GradientAlphaKey(0.5f, 0.5f),
                    new GradientAlphaKey(0.0f, 1.0f)
                }
            );
            trail.colorGradient = gradient;
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

    void Update()
    {
        if (hasExploded) return;

        // Apply rotation
        transform.Rotate(rotationSpeed * Time.deltaTime);

        // Apply pulsing scale animation
        if (enablePulsing)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = originalScale * pulse;
        }
    }

    // OnCollisionEnter removed - using trigger mode only

    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;

        // Don't explode if hitting player or weapon
        if (other.CompareTag("Player")) return;
        if (other.CompareTag("Weapon")) return;

        // Get object name for filtering
        string hitName = other.gameObject.name.ToLower();
        string hitTag = other.gameObject.tag;

        // IGNORE TERRAIN and environment objects - let projectile pass through
        if (hitName.Contains("terrain") ||
            hitName.Contains("rpgpp") ||
            hitName.Contains("ground") ||
            hitName.Contains("floor") ||
            other.gameObject.layer == LayerMask.NameToLayer("Terrain"))
        {
            return;
        }

        // Ignore skeleton body parts during the first 0.3 seconds
        float timeSinceSpawn = Time.time - spawnTime;
        bool isBodyPart = hitName.Contains("elbow") || hitName.Contains("shoulder") ||
                         hitName.Contains("knee") || hitName.Contains("hip") ||
                         hitName.Contains("wrist") || hitName.Contains("ankle") ||
                         hitName.Contains("spine") || hitName.Contains("neck") ||
                         hitName.Contains("finger") || hitName.Contains("toe");

        if (isBodyPart && timeSinceSpawn < 0.3f) return;

        // Check if this is an enemy or part of an enemy
        bool isEnemy = hitTag == "Enemy" || other.transform.root.CompareTag("Enemy");

        if (isEnemy)
        {
            // Find the actual skeleton root with EnemyHealth component
            GameObject targetEnemy = FindEnemyRoot(other.transform);

            if (targetEnemy != null)
            {
                Vector3 explosionPoint = other.ClosestPoint(transform.position);
                Explode(explosionPoint, targetEnemy);
            }
        }
        else if (hitTag == "Target")
        {
            Vector3 explosionPoint = other.ClosestPoint(transform.position);
            Explode(explosionPoint, null);
        }
    }

    GameObject FindEnemyRoot(Transform hitTransform)
    {
        // Walk up the hierarchy to find the GameObject with EnemyHealth component
        Transform current = hitTransform;
        while (current != null)
        {
            EnemyHealth enemyHealth = current.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                return current.gameObject;
            }
            current = current.parent;
        }

        // Fallback: just use the root
        return hitTransform.root.gameObject;
    }

    void Explode(Vector3 explosionPoint, GameObject hitEnemy)
    {
        hasExploded = true;

        // Create explosion visual effect
        if (explosionEffectPrefab != null)
        {
            GameObject explosion = Instantiate(explosionEffectPrefab, explosionPoint, Quaternion.identity);
            Destroy(explosion, 3f);
        }
        else if (GlobalReferences.instance != null && GlobalReferences.instance.explosionEffectPrefab != null)
        {
            GameObject explosion = Instantiate(GlobalReferences.instance.explosionEffectPrefab, explosionPoint, Quaternion.identity);
            Destroy(explosion, 2f);
        }

        // GUARANTEED: If we have a direct hit enemy, ONLY damage that specific one
        if (hitEnemy != null)
        {
            EnemyHealth enemyHealth = hitEnemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
            else
            {
                Destroy(hitEnemy);
            }

            // Apply explosion force ONLY to this enemy's ragdoll parts
            Rigidbody[] enemyRigidbodies = hitEnemy.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in enemyRigidbodies)
            {
                rb.AddExplosionForce(explosionForce, explosionPoint, explosionRadius);
            }

            // Destroy the projectile and exit - don't search for other enemies
            Destroy(gameObject);
            return;
        }
        // Only reach here if hitEnemy was null (shouldn't happen with proper collision detection)
        // Just destroy the projectile
        Destroy(gameObject);
    }

    // Optional: Visualize explosion radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
