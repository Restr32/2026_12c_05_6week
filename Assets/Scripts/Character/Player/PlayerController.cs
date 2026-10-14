using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float jumpForce = 12f;

    [Header("Ground Check")]
    [SerializeField] Transform groundCheck;
    [SerializeField] Vector2 groundCheckSize = new Vector2(0.72f, 0.2f);
    [SerializeField] LayerMask groundLayer;
    [Tooltip("Extra frames after leaving ground where jump still works.")]
    [SerializeField] float coyoteTime = 0.1f;
    // this variable is to check if you can do double jump
    bool DoubleJumpAvailable;

    Rigidbody2D rb;
    Collider2D col;
    float coyoteTimer;
    bool isGrounded;
    float moveInput;
    bool jumpQueued;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Update()
    {
        CheckGrounded();
        ReadInput();
    }

    void FixedUpdate()
    {
        ApplyMove();
        ApplyJump();
    }

    void CheckGrounded()
    {
        // Slightly generous OverlapBox — size + position leave room so
        // tiny physics gaps / contact offsets don't falsely report air.
        Vector2 checkPos = groundCheck != null
            ? (Vector2)groundCheck.position
            : (Vector2)transform.position + Vector2.down * 0.55f;

        isGrounded = Physics2D.OverlapBox(checkPos, groundCheckSize, 0f, groundLayer);

        if (isGrounded){
            coyoteTimer = coyoteTime;
            DoubleJumpAvailable = true;
        }
        else
            coyoteTimer -= Time.deltaTime;
    }

    void ReadInput()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null)
        {
            moveInput = 0f;
            return;
        }

        moveInput = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) moveInput -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) moveInput += 1f;

        if (kb.spaceKey.wasPressedThisFrame)
            jumpQueued = true;
    }

    void ApplyMove()
    {
        float targetX = moveInput * moveSpeed;

        // Don't shove into a wall — that friction fight is what kills mid-air Y velocity.
        if (moveInput != 0f && IsBlockedHorizontally(moveInput))
            targetX = 0f;

        Vector2 velocity = rb.linearVelocity;
        velocity.x = targetX;
        rb.linearVelocity = velocity;
    }

    void ApplyJump()
    {
        if (!jumpQueued) return;
        jumpQueued = false;

        if (coyoteTimer > 0f || DoubleJumpAvailable)
        {
            Vector2 velocity = rb.linearVelocity;
            velocity.y = jumpForce;
            rb.linearVelocity = velocity;
            if (coyoteTimer > 0f)
                coyoteTimer = 0f;
            else
                DoubleJumpAvailable = false;
        }
    }

    bool IsBlockedHorizontally(float direction)
    {
        Bounds bounds = col.bounds;
        // Slightly shorter than full height so floor/ceiling corners don't count as walls.
        Vector2 boxSize = new Vector2(0.1f, bounds.size.y * 0.8f);
        Vector2 origin = bounds.center;
        float distance = bounds.extents.x + 0.05f;

        RaycastHit2D hit = Physics2D.BoxCast(
            origin,
            boxSize,
            0f,
            new Vector2(Mathf.Sign(direction), 0f),
            distance,
            groundLayer);

        return hit.collider != null;
    }

    void OnDrawGizmosSelected()
    {
        Vector2 checkPos = groundCheck != null
            ? (Vector2)groundCheck.position
            : (Vector2)transform.position + Vector2.down * 0.55f;

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireCube(checkPos, groundCheckSize);
    }
}
