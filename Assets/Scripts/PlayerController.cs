using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Attributes")]
    [SerializeField] float movementSpeed = 10.0f;
    [SerializeField] float jumpForce = 16.0f;
    [SerializeField] int amountOfJumps = 1;
    [SerializeField] float wallSlidingSpeed;
    [SerializeField] float movementForceInAir;
    [SerializeField] float airDragMultiplier = 0.95f;
    [SerializeField] float variableJumpHeightMultiplier = 0.5f;
    [SerializeField] float wallHopForce;
    [SerializeField] float wallJumpForce;
    [SerializeField] Vector2 wallHopDirection;
    [SerializeField] Vector2 wallJumpDirection;
    [SerializeField] float jumpTimerSet = 0.15f;
    [SerializeField] float turnTimerSet = 0.1f;
    [SerializeField] float wallJumpTimerSet = 0.5f;


    [Header("Surrounding Check Attributes")]
    [SerializeField] float groundCheckRadius;
    [SerializeField] LayerMask whatIsGround;
    [SerializeField] float wallCheckDistance;
    [SerializeField] Vector2 ledgeClimbOffset1;
    [SerializeField] Vector2 ledgeClimbOffset2;


    [Header("References")]
    [SerializeField] Transform groundCheck;
    [SerializeField] Transform wallCheck;
    [SerializeField] Transform ledgeCheck;

    float movementInputDirection;
    bool isFacingRight = true;
    int facingDirection = 1;
    bool isWalking;
    bool isGrounded;
    bool canNormalJump;
    bool canWallJump;
    int amountOfJumpsLeft;
    bool isTouchingWall;
    bool isWallSliding;
    float jumpTimer;
    bool isAttemptingToJump;
    bool checkJumpMultiplier;
    bool canMove;
    bool canFlip;
    float turnTimer;
    float wallJumpTimer;
    bool hasWallJumped;
    int lastWallJumpDirection;
    bool isTouchingLedge;
    bool canClimbLedge = false;
    bool ledgeDetected;
    Vector2 ledgePosBottom;
    Vector2 ledgePos1;
    Vector2 ledgePos2;

    Rigidbody2D rb;
    Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        amountOfJumpsLeft = amountOfJumps;

        wallHopDirection.Normalize();
        wallJumpDirection.Normalize();
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
        CheckMovementDirection();
        UpdateAnimations();
        CheckIfCanJump();
        CheckIfWallSliding();
        CheckJump();
        // CheckLedgeClimb(); // TODO: To be added after bugfixes in part 25
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        CheckSurroundings();
    }

    private void CheckInput()
    {
        movementInputDirection = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded || (amountOfJumpsLeft > 0 && !isTouchingWall))
            {
                NormalJump();
            }
            else
            {
                jumpTimer = jumpTimerSet;
                isAttemptingToJump = true;
            }
        }

        if (Input.GetButtonDown("Horizontal") && isTouchingWall)
        {
            if (!isGrounded && movementInputDirection != facingDirection)
            {
                canMove = false;
                canFlip = false;

                turnTimer = turnTimerSet;
            }
        }

        if (turnTimer >= 0)
        {
            turnTimer -= Time.deltaTime;

            if (turnTimer <= 0)
            {
                canMove = true;
                canFlip = true;
            }
        }

        if (checkJumpMultiplier && !Input.GetButton("Jump"))
        {
            checkJumpMultiplier = false;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHeightMultiplier);
        }
    }

    private void ApplyMovement()
    {
        if (!isGrounded && !isWallSliding && movementInputDirection == 0)
        {
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);
        }
        else if (canMove)
        {
            rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y);
        }


        if (isWallSliding)
        {
            if (rb.velocity.y < -wallSlidingSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlidingSpeed);
            }
        }
    }

    private void CheckMovementDirection()
    {
        if (isFacingRight && movementInputDirection < 0)
        {
            Flip();
        }
        else if (!isFacingRight && movementInputDirection > 0)
        {
            Flip();
        }

        if (isGrounded && (rb.velocity.x < -0.01f || rb.velocity.x > 0.01f))
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }
    }

    private void Flip()
    {
        if (!isWallSliding && !canClimbLedge && canFlip)
        {
            facingDirection *= -1;
            isFacingRight = !isFacingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }
    }


    private void CheckJump()
    {
        if (jumpTimer > 0)
        {
            if (!isGrounded && isTouchingWall && movementInputDirection != 0 && movementInputDirection != facingDirection)
            {
                WallJump();
            }
            else if (isGrounded)
            {
                NormalJump();
            }
        }

        if (isAttemptingToJump)
        {
            jumpTimer -= Time.deltaTime;
        }

        if (wallJumpTimer > 0)
        {
            if (hasWallJumped && movementInputDirection == -lastWallJumpDirection)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0.0f);
                hasWallJumped = false;
            }
            else if (wallJumpTimer <= 0)
            {
                hasWallJumped = false;
            }
            else
            {
                wallJumpTimer -= Time.deltaTime;
            }
        }
    }

    private void NormalJump()
    {
        if (canNormalJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            amountOfJumpsLeft--;
            jumpTimer = 0;
            isAttemptingToJump = false;
            checkJumpMultiplier = true;
        }
    }

    private void WallJump()
    {
        if (canWallJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0.0f);
            isWallSliding = false;
            amountOfJumpsLeft = amountOfJumps;
            amountOfJumpsLeft--;
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * movementInputDirection, wallJumpForce * wallJumpDirection.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
            jumpTimer = 0;
            isAttemptingToJump = false;
            checkJumpMultiplier = true;
            turnTimer = 0;
            canMove = true;
            canFlip = true;
            hasWallJumped = true;
            wallJumpTimer = wallJumpTimerSet;
            lastWallJumpDirection = -facingDirection;
        }
    }

    private void CheckIfCanJump()
    {
        if (isGrounded && rb.velocity.y <= 0.01f)
        {
            amountOfJumpsLeft = amountOfJumps;
        }

        if (isTouchingWall)
        {
            canWallJump = true;
        }

        if (amountOfJumpsLeft <= 0)
        {
            canNormalJump = false;
        }
        else
        {
            canNormalJump = true;
        }
    }

    private void UpdateAnimations()
    {
        anim.SetBool("isWalking", isWalking);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isWallSliding", isWallSliding);
    }

    private void CheckSurroundings()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
        isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);
        isTouchingLedge = Physics2D.Raycast(ledgeCheck.position, transform.right, wallCheckDistance, whatIsGround);

        if (isTouchingWall && !isTouchingLedge && !ledgeDetected)
        {
            ledgeDetected = true;
            ledgePosBottom = wallCheck.position;
        }
    }

    private void CheckIfWallSliding()
    {
        if (isTouchingWall && movementInputDirection == facingDirection && rb.velocity.y < 0 && !canClimbLedge)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void CheckLedgeClimb()
    {
        if (ledgeDetected && !canClimbLedge)
        {
            canClimbLedge = true;

            if (isFacingRight)
            {
                ledgePos1 = new Vector2(Mathf.Floor(ledgePosBottom.x + wallCheckDistance) - ledgeClimbOffset1.x, Mathf.Floor(ledgePosBottom.y) + ledgeClimbOffset1.y);
                ledgePos2 = new Vector2(Mathf.Floor(ledgePosBottom.x + wallCheckDistance) + ledgeClimbOffset2.x, Mathf.Floor(ledgePosBottom.y) + ledgeClimbOffset2.y);
            }
            else
            {
                ledgePos1 = new Vector2(Mathf.Ceil(ledgePosBottom.x - wallCheckDistance) + ledgeClimbOffset1.x, Mathf.Floor(ledgePosBottom.y) + ledgeClimbOffset1.y);
                ledgePos2 = new Vector2(Mathf.Ceil(ledgePosBottom.x - wallCheckDistance) - ledgeClimbOffset2.x, Mathf.Floor(ledgePosBottom.y) + ledgeClimbOffset2.y);
            }

            canMove = false;
            canFlip = false;

            transform.position = ledgePos1;
            anim.SetBool("canClimbLedge", canClimbLedge);
        }


    }

    public void FinishLedgeClimb()
    {
        canClimbLedge = false;
        transform.position = ledgePos2;
        canMove = true;
        canFlip = true;
        ledgeDetected = false;
        anim.SetBool("canClimbLedge", canClimbLedge);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));

        Gizmos.DrawLine(ledgePos1, ledgePos2);
    }
}
