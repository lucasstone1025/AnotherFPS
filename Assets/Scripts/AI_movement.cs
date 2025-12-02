using UnityEngine;

public class AI_movement : MonoBehaviour
{
    public Transform player;           // Assign the player in the Inspector
    public float moveSpeed = 3.75f;    // Half of player's run speed (7.5 / 2)
    public float stoppingDistance = 2f; // How close to get before stopping
    public float obstacleDetectionRange = 1.5f; // How far ahead to check for obstacles
    public LayerMask obstacleLayer;    // What counts as an obstacle

    private CharacterController controller;

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
    }

    void Update()
    {
        if (player == null) return;

        // Calculate direction to player
        Vector3 direction = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check for obstacles ahead
        bool pathClear = !Physics.Raycast(transform.position + Vector3.up, direction, obstacleDetectionRange, obstacleLayer);

        // Move towards player if not within stopping distance and path is clear
        if (distanceToPlayer > stoppingDistance && pathClear)
        {
            // Move horizontally towards player
            Vector3 move = new Vector3(direction.x, 0, direction.z) * moveSpeed * Time.deltaTime;
            
            // Apply gravity
            move.y = -9.81f * Time.deltaTime;
            
            controller.Move(move);

            // Rotate to face player
            Vector3 lookDirection = new Vector3(direction.x, 0, direction.z);
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, 
                    Quaternion.LookRotation(lookDirection), 
                    Time.deltaTime * 5f);
            }
        }
    }
}
