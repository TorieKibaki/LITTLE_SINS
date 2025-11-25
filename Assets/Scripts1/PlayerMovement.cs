using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    // Removed public float maxJumpForce = 14f; as it is unused in the InstantJump implementation.

    // Private references
    private Animator anim;
    private Rigidbody2D rb;
    private bool isGrounded;

    void Start()
    {
        // 1. Get the components
        rb = GetComponent<Rigidbody2D>();

        // --- FIX START: More robust Animator search ---
        anim = GetComponent<Animator>(); // Try to find on the root object first
        if (anim == null)
        {
            // If not found, look on any child objects (e.g., a 'Graphics' model)
            anim = GetComponentInChildren<Animator>();
        }
        // --- FIX END ---

        // 2. Check for missing components and log warnings/errors
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component not found on Player! Movement will fail.");
        }
        if (anim == null)
        {
            // This warning is fine, but now it should only appear if no Animator is found anywhere.
            Debug.LogWarning("Animator component not found on Player! Animations will not work.");
        }
    }


    void Update()
    {
        float move = Input.GetAxis("Horizontal");

        // Horizontal Movement
        if (rb != null)
        {
            rb.velocity = new Vector2(move * moveSpeed, rb.velocity.y);
        }

        bool isMoving = Mathf.Abs(move) > 0.01f;

        // ANIMATION CONTROL: Only attempt to set animation parameters if the Animator exists
        if (anim != null)
        {
            anim.SetBool("IsWalking", isMoving);
        }

        // Facing Direction Control
        if (move > 0)
        {
            // Facing Right
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (move < 0)
        {
            // Facing Left
            transform.localScale = new Vector3(-1, 1, 1);
        }

        // JUMP INITIATION: Checks for button press and ground status, then calls the instant jump function
        if (Input.GetButtonDown("Jump") && isGrounded)
            InstantJump();
    }

    /**
     * Applies jump force instantly, eliminating the sluggish delay.
     */
    private void InstantJump()
    {
        // ANIMATION CONTROL: Only attempt to set animation parameters if the Animator exists
        if (anim != null)
        {
            anim.SetTrigger("Jump");
        }

        // Apply jump force immediately for instant response
        if (rb != null)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    // The old JumpRoutine IEnumerator has been removed.

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision surface is mostly flat and below the player (ground)
        if (collision.contacts[0].normal.y > 0.5f)
            isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }
}