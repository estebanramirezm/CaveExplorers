using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class AbilitySystem : MonoBehaviour
{
    [Header("Roll / Dash")]
    public float RollForce = 12f;
    public float RollDuration = 0.3f;
    public float RollCooldown = 0.8f;

    [Header("Crawl")]
    public float CrawlSpeed = 2f;
    public Vector3 CrawlScale = new Vector3(1f, 0.5f, 1f);

    [Header("Wall Climb")]
    public float WallClimbSpeed = 3f;
    public LayerMask WallLayer;
    public Transform WallCheck;
    public float WallCheckDistance = 0.3f;
    public KeyCode ClimbToggleKey = KeyCode.E;

    [Header("Grapple")]
    public GameObject GrappleHookPrefab;

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerVisuals visuals;

    private bool isTouchingWall;
    private bool isRolling;
    private bool rollOnCooldown;
    private bool isCrawlingMode;
    private bool isWallClimbing;

    private Vector3 normalScale;
    private float normalGravity;

    private GameObject activeGrapple;

    public bool IsRolling => isRolling;
    public bool IsCrawling => isCrawlingMode;
    public bool IsWallClimbing => isWallClimbing;
    public bool IsTouchingWall => isTouchingWall;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        visuals = GetComponentInChildren<PlayerVisuals>();

        normalScale = transform.localScale;
        normalGravity = rb.gravityScale;
    }

    public void HandleAbilityInput(bool isGrounded, float horizontalInput)
    {
        CheckWall(horizontalInput);

        HandleRoll(isGrounded);
        HandleWallClimb(isGrounded, horizontalInput);
        HandleCrawl(isGrounded, horizontalInput);

        UpdateAnimator(horizontalInput);
    }

    private void UpdateAnimator(float horizontalInput)
    {
        animator.SetBool("IsRolling", isRolling);
        animator.SetBool("IsCrawlMoving", isCrawlingMode && Mathf.Abs(horizontalInput) > 0.1f);
        animator.SetBool("IsWallClimbing", isWallClimbing);

        if (isWallClimbing)
        {
            animator.SetFloat("ClimbX", horizontalInput);
            animator.SetFloat("ClimbY", GetClimbInputY());
        }
        else
        {
            animator.SetFloat("ClimbX", 0f);
            animator.SetFloat("ClimbY", 0f);
        }
    }

    private void HandleRoll(bool isGrounded)
    {
        if (!Has("Roll")) return;

        if (isGrounded && !isRolling && !rollOnCooldown && Input.GetKeyDown(KeyCode.C))
        {
            StartCoroutine(RollRoutine());
            visuals?.FlashAbility("Roll");
        }
    }

    private IEnumerator RollRoutine()
    {
        isRolling = true;
        rollOnCooldown = true;

        float dir = Mathf.Sign(transform.localScale.x);
        rb.linearVelocity = new Vector2(dir * RollForce, rb.linearVelocity.y);

        float elapsed = 0f;

        while (elapsed < RollDuration)
        {
            rb.linearVelocity = new Vector2(dir * RollForce, rb.linearVelocity.y);
            elapsed += Time.deltaTime;
            yield return null;
        }

        isRolling = false;

        yield return new WaitForSeconds(RollCooldown);
        rollOnCooldown = false;
    }

    private void HandleCrawl(bool isGrounded, float horizontalInput)
    {
        if (isWallClimbing) return;

        if (!Has("Crawl"))
        {
            if (isCrawlingMode)
                EndCrawl();

            return;
        }

        if (!isGrounded) return;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (!isCrawlingMode)
                StartCrawl();
            else
                EndCrawl();
        }

        if (isCrawlingMode)
        {
            rb.linearVelocity = new Vector2(horizontalInput * CrawlSpeed, rb.linearVelocity.y);

            float direction = Mathf.Sign(transform.localScale.x);

            transform.localScale = new Vector3(
                direction * Mathf.Abs(CrawlScale.x),
                CrawlScale.y,
                CrawlScale.z
            );
        }
    }

    private void StartCrawl()
    {
        isCrawlingMode = true;
        animator.SetTrigger("StartCrawl");
        visuals?.FlashAbility("Crawl");
    }

    private void EndCrawl()
    {
        isCrawlingMode = false;
        animator.SetTrigger("EndCrawl");

        float direction = Mathf.Sign(transform.localScale.x);

        transform.localScale = new Vector3(
            direction * Mathf.Abs(normalScale.x),
            normalScale.y,
            normalScale.z
        );
    }

    private void CheckWall(float horizontalInput)
    {
        if (WallCheck == null)
        {
            isTouchingWall = false;
            return;
        }

        Vector2 dir;

        if (isWallClimbing)
            dir = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        else if (horizontalInput > 0)
            dir = Vector2.right;
        else if (horizontalInput < 0)
            dir = Vector2.left;
        else
            dir = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        isTouchingWall = Physics2D.Raycast(
            WallCheck.position,
            dir,
            WallCheckDistance,
            WallLayer
        );
    }

    private void HandleWallClimb(bool isGrounded, float horizontalInput)
    {
        if (!Has("WallClimb"))
        {
            StopWallClimb();
            return;
        }

        float climbY = GetClimbInputY();

        if (!isWallClimbing)
        {
            if (isTouchingWall && !isGrounded && climbY > 0)
                StartWallClimb();
        }

        if (isWallClimbing)
        {
            if (!isTouchingWall || isGrounded)
            {
                StopWallClimb();
                return;
            }

            if (Input.GetKeyDown(ClimbToggleKey))
            {
                StopWallClimb();
                return;
            }

            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(0f, climbY * WallClimbSpeed);
        }
    }

    private void StartWallClimb()
    {
        if (isCrawlingMode)
            EndCrawl();

        isWallClimbing = true;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;

        visuals?.FlashAbility("WallClimb");
    }

    private void StopWallClimb()
    {
        if (!isWallClimbing) return;

        isWallClimbing = false;
        rb.gravityScale = normalGravity;
    }

    private float GetClimbInputY()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            return 1f;

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            return -1f;

        return 0f;
    }

    private bool Has(string abilityName)
    {
        return GameManager.Instance != null && GameManager.Instance.HasAbility(abilityName);
    }
}