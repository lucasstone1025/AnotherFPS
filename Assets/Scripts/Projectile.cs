using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject explosionEffectPrefab;
    public float explosionRadius = 3f;
    public float explosionForce = 700f;
    public float damage = 1000f;
    public float gravityScale = 0.05f;

    // Visual settings
    public Vector3 rotationSpeed = new Vector3(0, 360f, 0);
    public bool enablePulsing = true;
    public float pulseSpeed = 3f;
    public float pulseAmount = 0.2f;
    public Color lightColor = new Color(1f, 0.5f, 0f);
    public float lightIntensity = 3f;
    public float lightRange = 8f;

    private TrailRenderer trail;
    private Light projectileLight;
    private Vector3 originalScale;
    private bool hasExploded = false;
    private Rigidbody rb;

    void Start()
    {
        originalScale = transform.localScale;

        // Setup rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        // Setup collider as trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Ignore player collisions only
        IgnorePlayerCollisions();

        // Setup visuals
        SetupLight();
        SetupTrail();

        Debug.Log("Projectile spawned and ready");
    }

    void IgnorePlayerCollisions()
    {
        Collider thisCollider = GetComponent<Collider>();
        if (thisCollider == null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            foreach (Collider col in player.GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(thisCollider, col);
            }
        }
    }

    void SetupLight()
    {
        projectileLight = GetComponent<Light>();
        if (projectileLight == null)
            projectileLight = gameObject.AddComponent<Light>();

        projectileLight.type = LightType.Point;
        projectileLight.color = lightColor;
        projectileLight.intensity = lightIntensity;
        projectileLight.range = lightRange;
        projectileLight.shadows = LightShadows.None;
    }

    void SetupTrail()
    {
        trail = GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
            trail.time = 0.7f;
            trail.startWidth = 0.3f;
            trail.endWidth = 0.05f;

            Shader trailShader = Shader.Find("Sprites/Default");
            if (trailShader != null)
            {
                trail.material = new Material(trailShader);
            }

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(1f, 0.6f, 0f), 0.0f),
                    new GradientColorKey(new Color(1f, 0.3f, 0f), 0.5f),
                    new GradientColorKey(Color.red, 1.0f)
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
        if (rb != null && !hasExploded)
        {
            rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);
        }
    }

    void Update()
    {
        if (hasExploded) return;

        transform.Rotate(rotationSpeed * Time.deltaTime);

        if (enablePulsing)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = originalScale * pulse;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;

        // Skip player and weapon
        if (other.CompareTag("Player") || other.CompareTag("Weapon")) return;

        // Check for SkeletonHitbox component directly - most reliable method
        SkeletonHitbox hitbox = other.GetComponent<SkeletonHitbox>();
        if (hitbox != null)
        {
            // Find EnemyHealth - might be on this object or a parent
            GameObject enemyRoot = FindEnemyWithHealth(hitbox.gameObject);
            if (enemyRoot != null)
            {
                Debug.Log("*** HIT SKELETON via Hitbox! Enemy: " + enemyRoot.name + " ***");
                HitEnemy(enemyRoot);
            }
            else
            {
                Debug.LogWarning("SkeletonHitbox hit but no EnemyHealth found on: " + hitbox.gameObject.name);
            }
            return;
        }

        // Also check if we hit something tagged Enemy or with Enemy root
        if (other.CompareTag("Enemy") || other.transform.root.CompareTag("Enemy"))
        {
            // Find the root with EnemyHealth
            Transform current = other.transform;
            while (current != null)
            {
                EnemyHealth health = current.GetComponent<EnemyHealth>();
                if (health != null)
                {
                    Debug.Log("*** HIT ENEMY: " + current.name + " ***");
                    HitEnemy(current.gameObject);
                    return;
                }
                current = current.parent;
            }
        }

        // Ignore terrain and props - just pass through
        // No need to do anything, projectile continues flying
    }

    GameObject FindEnemyWithHealth(GameObject startObject)
    {
        // Search this object and all parents for EnemyHealth
        Transform current = startObject.transform;
        while (current != null)
        {
            EnemyHealth health = current.GetComponent<EnemyHealth>();
            if (health != null)
            {
                return current.gameObject;
            }
            current = current.parent;
        }
        return null;
    }

    void HitEnemy(GameObject enemy)
    {
        hasExploded = true;

        // Create explosion effect
        if (explosionEffectPrefab != null)
        {
            GameObject explosion = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 3f);
        }
        else if (GlobalReferences.instance != null && GlobalReferences.instance.explosionEffectPrefab != null)
        {
            GameObject explosion = Instantiate(GlobalReferences.instance.explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 2f);
        }

        // Deal damage
        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }

        // Apply explosion force
        foreach (Rigidbody enemyRb in enemy.GetComponentsInChildren<Rigidbody>())
        {
            enemyRb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
        }

        // Hide the projectile visually but keep alive briefly for trail to fade
        HideProjectile();
        Destroy(gameObject, 0.3f);  // Small delay so trail doesn't pop out of existence
    }

    void HideProjectile()
    {
        // Hide mesh renderer
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }

        // Turn off light
        if (projectileLight != null)
        {
            projectileLight.enabled = false;
        }

        // Stop rigidbody movement
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // Disable collider to prevent any more hits
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
