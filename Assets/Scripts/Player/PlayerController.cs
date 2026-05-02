using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float MoveSpeed = 5f;
    public float JumpForce = 10f;

    [Header("Ground Check")]
    public Transform GroundCheck;
    public float GroundCheckRadius = 0.15f;
    public LayerMask GroundLayer;
    
    // Added this variable for the GrappleBat to access!
    [HideInInspector] 
    public bool IsGrabbed;

    private Rigidbody2D rb;
    private Animator animator;
    private HealthManager health;
    private AbilitySystem abilitySystem;
    private PlayerVisuals visuals;
    private FireflySwarm fireflySwarm;

    private bool isGrounded;
    private float horizontalInput;
    private bool wasRunning;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        health = GetComponent<HealthManager>();
        abilitySystem = GetComponent<AbilitySystem>();
        visuals = GetComponent<PlayerVisuals>();
        fireflySwarm = GetComponent<FireflySwarm>();
    }

    void Update()
    {
        // If the bat has grabbed the player, stop updating player inputs
        if (IsGrabbed) return;

        horizontalInput = Input.GetAxisRaw("Horizontal");

        CheckGround();

        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("YVelocity", rb.linearVelocity.y);

        UpdateWallHoldAnimation();

        HandleJump();

        abilitySystem?.HandleAbilityInput(isGrounded, horizontalInput);

        animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
    }

    void FixedUpdate()
    {
        Move();
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(
            GroundCheck.position,
            GroundCheckRadius,
            GroundLayer
        );

        if (isGrounded)
            health?.SetSafePosition(transform.position);
    }

    private void Move()
    {
        // Prevent normal movement if being carried by a bat
        if (IsGrabbed) return;

        if (fireflySwarm != null && fireflySwarm.IsLuring)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        var grapple = GetComponent<GrappleHook>();
        bool isGrappling = grapple != null && grapple.IsGrappling;

        if (isGrappling)
        {
            rb.AddForce(new Vector2(horizontalInput * 3f, 0f));
            return;
        }

        if (abilitySystem != null &&
            (abilitySystem.IsRolling || abilitySystem.IsWallClimbing || abilitySystem.IsCrawling))
        {
            return;
        }

        float speed = MoveSpeed;
        bool isRunning = false;

        if (GameManager.Instance != null && GameManager.Instance.HasAbility("Run"))
        {
            if (Input.GetKey(KeyCode.LeftShift) && horizontalInput != 0)
            {
                speed *= 2f;
                isRunning = true;
            }
        }

        if (isRunning && !wasRunning)
            visuals?.FlashAbility("Run");

        wasRunning = isRunning;

        rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);

        if (horizontalInput != 0)
        {
            transform.localScale = new Vector3(
                Mathf.Sign(horizontalInput) * Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
        }
    }

    private void HandleJump()
    {
        // Prevent jumping if being carried by a bat
        if (IsGrabbed) return;

        if (fireflySwarm != null && fireflySwarm.IsLuring)
            return;

        if (!isGrounded) return;

        if (abilitySystem != null &&
            (abilitySystem.IsRolling || abilitySystem.IsWallClimbing || abilitySystem.IsCrawling))
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetTrigger("JumpPressed");
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpForce);
        }
    }

    private void UpdateWallHoldAnimation()
    {
        bool isWallHolding =
            !isGrounded &&
            abilitySystem != null &&
            abilitySystem.IsTouchingWall &&
            Mathf.Abs(horizontalInput) > 0.1f &&
            rb.linearVelocity.y <= 0.1f &&
            !abilitySystem.IsWallClimbing;

        animator.SetBool("IsWallHolding", isWallHolding);
    }

    public void ReceiveDamage(int amount, Transform source)
    {
        visuals?.FlashDamage();
        health?.TakeDamageFrom(amount, source);
    }

    public void ReceiveDamage(int amount = 1)
    {
        visuals?.FlashDamage();
        health?.TakeDamage(amount);
    }

    void OnDrawGizmosSelected()
    {
        if (GroundCheck == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(GroundCheck.position, GroundCheckRadius);
    }
}