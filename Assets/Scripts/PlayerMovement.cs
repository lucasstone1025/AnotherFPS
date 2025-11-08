using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller; 

    public float speed = 12f;
    public float gravity = -9.81f * 2;
    public float jumpHeight = 3f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    Vector3 velocity;
    bool isMoving;

    private Vector3 lastPosition = new Vector3(0f, 0f, 0f);

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        //ground check
        bool isGrounded = controller.isGrounded;

        //reset default velocity 
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; //dont let jump build up
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");


        //create movement vector
        Vector3 move = transform.right * x + transform.forward * z; //(right/left) + (forward/backward

        // actually move the player
        controller.Move(move * speed * Time.deltaTime);

        //jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        //fall down
        velocity.y += gravity * Time.deltaTime;

        // executing jump
        controller.Move(velocity * Time.deltaTime);

        if (lastPosition != gameObject.transform.position && isGrounded == true)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
        
        lastPosition = gameObject.transform.position;


    }
}
