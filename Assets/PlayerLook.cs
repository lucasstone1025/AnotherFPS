using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [Header("References")]
    public Transform PlayerCamera;

    [Header("Settings")]
    public Vector2 Sensitivities = new Vector2(1.0f, 1.0f);

    private Vector2 XYRotation;

    void Start()
    {
        // Hide and lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // --- New Input System mouse read ---
        Vector2 mouseDelta = Vector2.zero;

        if (Mouse.current != null)
        {
            mouseDelta = Mouse.current.delta.ReadValue();
        }

        // Apply sensitivity
        XYRotation.x -= mouseDelta.y * Sensitivities.y * Time.deltaTime;
        XYRotation.y += mouseDelta.x * Sensitivities.x * Time.deltaTime;

        // Clamp vertical rotation
        XYRotation.x = Mathf.Clamp(XYRotation.x, -90f, 90f);

        // Apply rotations
        transform.localRotation = Quaternion.Euler(0f, XYRotation.y, 0f);
        if (PlayerCamera != null)
            PlayerCamera.localRotation = Quaternion.Euler(XYRotation.x, 0f, 0f);
    }
}
