using UnityEngine;
using UnityEngine.InputSystem; // New Input System

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float MoveSmoothTime = 0.08f;
    public float GravityStrength = 20f;
    public float JumpStrength = 6.5f;
    public float WalkSpeed = 4f;
    public float RunSpeed = 7.5f;

    private CharacterController controller;
    private Vector3 currentMoveVelocity;   // horizontal velocity (x,z)
    private Vector3 moveDampVelocity;      // smoothing ref
    private float verticalVelocity;        // y only

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // --- INPUT (new Input System) ---
        float hx = 0f, vz = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed) hx -= 1f;
            if (Keyboard.current.dKey.isPressed) hx += 1f;
            if (Keyboard.current.sKey.isPressed) vz -= 1f;
            if (Keyboard.current.wKey.isPressed) vz += 1f;
        }
        else if (Gamepad.current != null)
        {
            hx = Gamepad.current.leftStick.x.ReadValue();
            vz = Gamepad.current.leftStick.y.ReadValue();
        }

        Vector3 input = new Vector3(hx, 0f, vz);
        if (input.sqrMagnitude > 1f) input.Normalize();

        bool runHeld =
            (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed) ||
            (Gamepad.current != null && Gamepad.current.leftStickButton.isPressed);

        float targetSpeed = runHeld ? RunSpeed : WalkSpeed;

        // world-space horizontal move
        Vector3 targetMove = transform.TransformDirection(input) * targetSpeed;

        // Smooth horizontal velocity (x,z only)
        currentMoveVelocity = Vector3.SmoothDamp(
            currentMoveVelocity,
            new Vector3(targetMove.x, 0f, targetMove.z),
            ref moveDampVelocity,
            MoveSmoothTime
        );

        // --- GROUND / JUMP ---
        bool grounded = controller.isGrounded;

        if (grounded)
        {
            // keep grounded
            if (verticalVelocity < 0f) verticalVelocity = -2f;

            bool jumpPressed =
                (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
                (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);

            if (jumpPressed)
            {
                verticalVelocity = JumpStrength;
            }
        }

        // gravity (always)
        verticalVelocity -= GravityStrength * Time.deltaTime;

        // --- SINGLE MOVE CALL ---
        Vector3 motion = new Vector3(currentMoveVelocity.x, verticalVelocity, currentMoveVelocity.z) * Time.deltaTime;
        controller.Move(motion);
    }
}
