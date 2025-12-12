using UnityEngine;

// Add this script to your Skeleton prefab to give it a simple, easy-to-hit trigger collider
public class SkeletonHitbox : MonoBehaviour
{
    [Header("Hitbox Settings")]
    public float hitboxRadius = 1.5f;
    public Vector3 hitboxCenter = new Vector3(0, 1.0f, 0);

    private SphereCollider hitboxCollider;

    void Awake()
    {
        // Use Awake instead of Start so the collider exists before projectiles check
        SetupHitbox();
    }

    void SetupHitbox()
    {
        // Check if we already have a sphere collider
        hitboxCollider = GetComponent<SphereCollider>();

        if (hitboxCollider == null)
        {
            hitboxCollider = gameObject.AddComponent<SphereCollider>();
        }

        // Configure as trigger - BOTH projectile and hitbox are triggers
        // Unity allows trigger-to-trigger collisions when one has a Rigidbody
        hitboxCollider.isTrigger = true;
        hitboxCollider.radius = hitboxRadius;
        hitboxCollider.center = hitboxCenter;

        Debug.Log("SkeletonHitbox ready: trigger collider with radius " + hitboxRadius);
    }

    // Optional: Visualize the hitbox in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position + hitboxCenter, hitboxRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + hitboxCenter, hitboxRadius);
    }
}
