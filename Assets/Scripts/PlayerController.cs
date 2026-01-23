using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed of the player.")]
    public float moveSpeed = 6.0f;

    [Tooltip("Rotation speed when turning.")]
    public float rotationSpeed = 10.0f;
    
    private Rigidbody rb;

    // To handle camera-relative movement
    private Transform cameraTransform;
    
    // Input storage
    private float horizontalInput;
    private float verticalInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Ensure the Rigidbody doesn't tip over
        rb.freezeRotation = true;

        // Try to find the main camera
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        // Get Inputs in Update
        horizontalInput = 0f;
        verticalInput = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontalInput -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontalInput += 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) verticalInput += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) verticalInput -= 1f;
        }
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
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

            moveDirection = forward * verticalInput + right * horizontalInput;
        }
        else
        {
            // World-relative movement (fallback)
            moveDirection = new Vector3(horizontalInput, 0f, verticalInput);
        }

        // Apply movement to Rigidbody
        if (moveDirection.magnitude >= 0.1f)
        {
            // Normalize moveDirection if needed, but here we just want direction
            Vector3 desiredVelocity = moveDirection.normalized * moveSpeed;
            
            // Preserve vertical velocity (gravity)
            desiredVelocity.y = rb.linearVelocity.y;
            
            rb.linearVelocity = desiredVelocity;

            // Rotate character to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }
        else
        {
            // Stop horizontal movement when no input, but keep gravity
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }
}
