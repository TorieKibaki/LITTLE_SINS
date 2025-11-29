using UnityEngine;
// 👇 VITAL: Needed for the new Input System
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    private Rigidbody2D rb;
    private bool isGrounded = true;

    // New variable to store the joystick value
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // ================================================================
    // NEW INPUT SYSTEM FUNCTIONS (Link these in the Inspector!)
    // ================================================================

    // Called automatically when you use the Joystick (or WASD)
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    // Called automatically when you press the Jump Button
    public void OnJump(InputAction.CallbackContext context)
    {
        // context.performed means "Button was just pressed"
        if (context.performed && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
        }
    }
    // ================================================================

    void FixedUpdate()
    {
        // We now use the variable 'moveInput.x' instead of Input.GetAxis
        rb.velocity = new Vector2(moveInput.x * moveSpeed, rb.velocity.y);
    }

    // Keep your existing ground check logic
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}