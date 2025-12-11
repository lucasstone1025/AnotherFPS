using UnityEngine;
using UnityEngine.Video;

public class AI_movement : MonoBehaviour
{
    //Target
    public Transform player;           // Assign the player in the Inspector
    //Movement Settings
    public float moveSpeed = 3.75f;    // Half of player's run speed (7.5 / 2)
    public float stoppingDistance = 2f; // How close to get before stopping
    public float rotationSpeed = 5f;   // Speed of rotation towards player
    public float obstacleDetectionRange = 1.5f; // How far ahead to check for obstacles
    public LayerMask obstacleLayer;    // What counts as an obstacle

    //Attack Settings
    public float attackRange = 2f;    // Distance to initiate attack
    public float attackDamage = 10f; // Damage dealt per attack
    public float attackCooldown = 1.5f; // Time between attacks
    private float lastAttackTime;

    //Ground Check
    public float groundCheckDistance = 0.3f; // Distance to check for ground
    public LayerMask groundLayer;        // What counts as ground
    private CharacterController controller;
    private float verticalVelocity;
    private const float GRAVITY = -9.81f;

    //Movement Soothing
    private Vector3 currentVelocity;
    private Vector3 velocitySmoothing;
    public float accelerationTime = 0.15f;

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

        // Get or add CharacterController
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
        }

        // Configure CharacterController for smoother movement
        controller.skinWidth = 0.08f;
        controller.minMoveDistance = 0.001f;
    }

    void Update()
    {
        if (player == null) {
            Debug.Log("Player is NULL");
            return;
        }

        // // Calculate direction to player
        // Vector3 direction = (player.position - transform.position).normalized;
        // float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // // Check for obstacles ahead
        // bool pathClear = !Physics.Raycast(transform.position + Vector3.up, direction, obstacleDetectionRange, obstacleLayer);

        // // Move towards player if not within stopping distance and path is clear
        // if (distanceToPlayer > stoppingDistance && pathClear)
        // {
        //     // Move horizontally towards player
        //     Vector3 move = new Vector3(direction.x, 0, direction.z) * moveSpeed * Time.deltaTime;
            
        //     // Apply gravity
        //     move.y = -9.81f * Time.deltaTime;
            
        //     controller.Move(move);

        //     // Rotate to face player
        //     Vector3 lookDirection = new Vector3(direction.x, 0, direction.z);
        //     if (lookDirection != Vector3.zero)
        //     {
        //         transform.rotation = Quaternion.Slerp(transform.rotation, 
        //             Quaternion.LookRotation(lookDirection), 
        //             Time.deltaTime * 5f);
        //     }
        // }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        //Debug.Log("Distance to player: " + distanceToPlayer + " | Attack Range: " + attackRange + " | Stopping Distance: " + stoppingDistance);

        //Attack if in range
        if (distanceToPlayer <= attackRange)
        {
            tryAttack();
        }

        //handle movement
        HandleMovement(distanceToPlayer);

        //always face the player (smooth rotation)
        RotateTowardsPlayer();
    }

    void HandleMovement(float distanceToPlayer)
    {
        //Calculate direction to player
        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0; // Ignore vertical difference
        directionToPlayer.Normalize();

        //Check for obstacles ahead
        bool pathClear = !Physics.Raycast(
            transform.position + Vector3.up * 0.05f,
            directionToPlayer, 
            obstacleDetectionRange, 
            obstacleLayer);
        
        //Determine target velocity
        Vector3 targetVelocity = Vector3.zero;
        if (distanceToPlayer > stoppingDistance && pathClear)
        {
            targetVelocity = directionToPlayer * moveSpeed;
        }

        // smooth horizontal movement
        currentVelocity = Vector3.SmoothDamp(
            currentVelocity, 
            targetVelocity, 
            ref velocitySmoothing, 
            accelerationTime
        );

        //handle gravity
        if (controller.isGrounded)
        {
            verticalVelocity = -2f; // Small downward force to keep grounded
        }
        else
        {
            verticalVelocity += GRAVITY * Time.deltaTime;
        }

        //combine horizontal and vertical movement
        Vector3 movement = new Vector3(
            currentVelocity.x, 
            verticalVelocity, 
            currentVelocity.z
        ) * Time.deltaTime;

        //apply movement
        controller.Move(movement);
    }

    void RotateTowardsPlayer()
    {
        Vector3 lookDirection = player.position - transform.position;
        lookDirection.y = 0; // Keep only horizontal direction

        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                Time.deltaTime * rotationSpeed
            );
        }
    }

    void tryAttack()
    {
        //check attack cooldown
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        lastAttackTime = Time.time;

        //Deal damage to player
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.TakeDamage(attackDamage);
            Debug.Log("AI attacked player for " + attackDamage + " damage.");
        }
    }

    //Visualize attack range in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }
}
