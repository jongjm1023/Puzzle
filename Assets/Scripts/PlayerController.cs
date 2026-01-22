using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed of the player.")]
    public float moveSpeed = 6.0f;

    [Tooltip("Rotation speed when turning.")]
    public float rotationSpeed = 10.0f;

    [Header("Gravity Settings")]
    [Tooltip("The strength of gravity to apply.")]
    public float gravity = -9.81f;

    [Tooltip("Should gravity be applied?")]
    public bool useGravity = true;

    [Tooltip("Velocity dampening when hitting ground.")]
    public float groundedGravity = -2.0f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    // To handle camera-relative movement
    private Transform cameraTransform;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // Try to find the main camera
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        HandleMovement();
        HandleGravity();
    }

    void HandleMovement()
    {
        // Get Inputs using Unity Input System
        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;
        }

        Vector3 moveDirection = Vector3.zero;

        if (cameraTransform != null)
        {
            // Camera-relative movement
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            // Flatten to XZ plane so looking up/down doesn't affect speed
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            moveDirection = forward * vertical + right * horizontal;
        }
        else
        {
            // World-relative movement (fallback)
            moveDirection = new Vector3(horizontal, 0f, vertical);
        }

        // Move the player
        if (moveDirection.magnitude >= 0.1f)
        {
            // Normalize moveDirection so diagonal movement isn't faster
            // But only if we have input (magnitude > 0)
            controller.Move(moveDirection.normalized * moveSpeed * Time.deltaTime);

            // Optional: Rotate character to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void HandleGravity()
    {
        if (!useGravity) return;

        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = groundedGravity;
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    // ==========================================
    // Public Functions to Control Gravity
    // ==========================================

    /// <summary>
    /// Sets the gravity value directly.
    /// </summary>
    /// <param name="newGravity">New gravity value (e.g., -9.81).</param>
    public void SetGravity(float newGravity)
    {
        gravity = newGravity;
    }

    /// <summary>
    /// Multiplies the current gravity by a factor.
    /// </summary>
    /// <param name="multiplier">Factor to multiply gravity by.</param>
    public void MultiplyGravity(float multiplier)
    {
        gravity *= multiplier;
    }

    /// <summary>
    /// Enables or disables gravity application.
    /// </summary>
    /// <param name="enabled">True to enable, false to disable.</param>
    public void SetGravityEnabled(bool enabled)
    {
        useGravity = enabled;
        if (!enabled)
        {
            velocity.y = 0f; // Reset vertical velocity when gravity is disabled
        }
    }
}
