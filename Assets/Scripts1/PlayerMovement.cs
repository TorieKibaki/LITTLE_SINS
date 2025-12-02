using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float clampedJumpForce = 5f;       // Reduced jump force
    public float jumpHeightThreshold = 1.0f;  // Y-coordinate cutoff

    [Header("Components")]
    public Rigidbody2D rb;
    public Transform groundCheck;
    public LayerMask groundLayer;

    // Internal variables
    private Vector2 moveInput;
    private bool isGrounded;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }

    // --- INPUT SYSTEM FUNCTIONS ---

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            float currentYPosition = transform.position.y;
            float finalJumpForce;

            // CHECK THE PLAYER'S HEIGHT AND CLAMP THE JUMP FORCE
            if (currentYPosition >= jumpHeightThreshold)
            {
                // Use the reduced jump force
                finalJumpForce = clampedJumpForce;
            }
            else
            {
                // Use the normal jump force
                finalJumpForce = jumpForce;
            }

            // Apply the chosen velocity
            rb.velocity = new Vector2(rb.velocity.x, finalJumpForce);
        }
    }

    // --- PHYSICS LOOP ---

    private void FixedUpdate()
    {
        // 1. Ground Check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

        // 2. Apply Movement
        rb.velocity = new Vector2(moveInput.x * moveSpeed, rb.velocity.y);

        // 3. FLIP CHARACTER DIRECTION 🔄
        if (moveInput.x > 0)
        {
            // Moving Right -> Face Right (Scale 1)
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (moveInput.x < 0)
        {
            // Moving Left -> Face Left (Scale -1)
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
}