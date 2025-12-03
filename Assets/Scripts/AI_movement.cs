using UnityEngine;

public class AI_movement : MonoBehaviour
{
    public Transform player;           // Assign the player in the Inspector
    public float moveSpeed = 3.75f;    // Half of player's run speed (7.5 / 2)
    public float stoppingDistance = 2f; // How close to get before stopping
    public float rotationSpeed = 3f;   // How fast to rotate toward player

    private Rigidbody rb;

    void Start()
    {
        // Try to find the player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        // Get or add Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Configure Rigidbody to prevent physics issues
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = true;
        rb.mass = 1f;
        rb.linearDamping = 5f; // Add drag to prevent sliding
    }

    void FixedUpdate()
    {
        if (player == null || rb == null) return;

        // Calculate direction to player (only on horizontal plane)
        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > stoppingDistance)
        {
            // Normalize direction
            directionToPlayer.Normalize();

            // Smoothly rotate to face the player
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

            // Move forward
            Vector3 moveVelocity = directionToPlayer * moveSpeed;
            moveVelocity.y = rb.linearVelocity.y; // Preserve vertical velocity for gravity
            rb.linearVelocity = moveVelocity;
        }
        else
        {
            // Stop horizontal movement when close enough
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }
}
