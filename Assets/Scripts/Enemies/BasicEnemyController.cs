using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyController : MonoBehaviour
{
    enum State
    {
        Moving,
        Knockback,
        Dead
    }

    [SerializeField] float movementSpeed;
    [SerializeField] float groundCheckDistance;
    [SerializeField] float wallCheckDistance;
    [SerializeField] Transform groundCheck;
    [SerializeField] Transform wallCheck;
    [SerializeField] LayerMask whatIsGround;
    [SerializeField] float maxHealth;
    [SerializeField] float knockbackDuration;
    [SerializeField] Vector2 knockbackVelocity;
    [SerializeField] GameObject hitParticle;
    [SerializeField] GameObject deathChunkParticle;
    [SerializeField] GameObject deathBloodParticle;
    [SerializeField] float touchDamageCooldownTime;
    [SerializeField] Transform touchDamageCheck;
    [SerializeField] float touchDamage;
    [SerializeField] float touchDamageWidth;
    [SerializeField] float touchDamageHeight;
    [SerializeField] LayerMask whatIsPlayer;


    State currentState;
    bool groundDetected;
    bool wallDetected;
    int facingDirection;
    GameObject aliveGO;
    Rigidbody2D aliveRb;
    Animator aliveAnim;
    Vector2 movement;
    float currentHealth;
    float knockbackStartTime;
    int damageDirection;
    Vector2 touchDamageBottomLeft;
    Vector2 touchDamageTopRight;
    AttackDetails attackDetails;
    float lastTouchDamageTime = Mathf.NegativeInfinity;


    private void Start()
    {
        aliveGO = transform.Find("Alive").gameObject;
        aliveRb = aliveGO.GetComponent<Rigidbody2D>();
        aliveAnim = aliveGO.GetComponent<Animator>();

        facingDirection = 1;
        currentHealth = maxHealth;
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Moving:
                UpdateWalkingState();
                break;
            case State.Knockback:
                UpdateKnockbackState();
                break;
            case State.Dead:
                UpdateDeadState();
                break;
            default:
                break;
        }
    }

    //  --MOVING STATE-------------

    private void EnterMovingState()
    {

    }

    private void UpdateWalkingState()
    {
        groundDetected = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
        wallDetected = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);

        CheckTouchDamage();

        if (!groundDetected || wallDetected)
        {
            Flip();
        }
        else
        {
            movement.Set(movementSpeed * facingDirection, aliveRb.velocity.y);
            aliveRb.velocity = movement;
        }
    }

    private void ExitMovingState()
    {

    }

    //  --KNOCKBACK STATE-------------

    private void EnterKnockbackState()
    {
        knockbackStartTime = Time.time;
        movement.Set(knockbackVelocity.x * damageDirection, knockbackVelocity.y);
        aliveRb.velocity = movement;
        aliveAnim.SetBool("Knockback", true);
    }

    private void UpdateKnockbackState()
    {
        if (Time.time >= knockbackStartTime + knockbackDuration)
        {
            SwitchState(State.Moving);
        }
    }

    private void ExitKnockbackState()
    {
        aliveAnim.SetBool("Knockback", false);
    }

    //  --DEAD STATE-------------
    private void EnterDeadState()
    {
        Instantiate(deathChunkParticle, aliveGO.transform.position, deathChunkParticle.transform.rotation);
        Instantiate(deathBloodParticle, aliveGO.transform.position, deathBloodParticle.transform.rotation);

        Destroy(gameObject);
    }

    private void UpdateDeadState()
    {

    }

    private void ExitDeadState()
    {

    }

    // --OTHER FUNCTIONS-------

    private void SwitchState(State state)
    {
        switch (currentState)
        {
            case State.Moving:
                ExitMovingState();
                break;
            case State.Knockback:
                ExitKnockbackState();
                break;
            case State.Dead:
                ExitDeadState();
                break;
            default:
                break;
        }

        switch (state)
        {
            case State.Moving:
                EnterMovingState();
                break;
            case State.Knockback:
                EnterKnockbackState();
                break;
            case State.Dead:
                EnterDeadState();
                break;
            default:
                break;
        }

        currentState = state;
    }

    private void Flip()
    {
        facingDirection *= -1;
        aliveGO.transform.Rotate(0.0f, 180.0f, 0.0f);
    }

    private void Damage(AttackDetails attackDetails)
    {
        currentHealth -= attackDetails.damageAmount;

        Instantiate(hitParticle, aliveGO.transform.position, Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)));

        if (attackDetails.position.x > aliveGO.transform.position.x)
        {
            damageDirection = -1;
        }
        else
        {
            damageDirection = 1;
        }

        if (currentHealth > 0.0f)
        {
            SwitchState(State.Knockback);
        }
        else if (currentHealth <= 0.0f)
        {
            SwitchState(State.Dead);
        }
    }

    private void CheckTouchDamage()
    {
        if (Time.time >= lastTouchDamageTime + touchDamageCooldownTime)
        {
            touchDamageBottomLeft.Set(touchDamageCheck.position.x - (touchDamageWidth / 2), touchDamageCheck.position.y - (touchDamageHeight / 2));
            touchDamageTopRight.Set(touchDamageCheck.position.x + (touchDamageWidth / 2), touchDamageCheck.position.y + (touchDamageHeight / 2));

            Collider2D hit = Physics2D.OverlapArea(touchDamageBottomLeft, touchDamageTopRight, whatIsPlayer);

            if (hit != null)
            {
                lastTouchDamageTime = Time.time;
                attackDetails.damageAmount = touchDamage;
                attackDetails.position = aliveGO.transform.position;
                hit.SendMessage("Damage", attackDetails);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundCheck.position, new Vector2(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        Gizmos.DrawLine(wallCheck.position, new Vector2(wallCheck.position.x + wallCheckDistance, wallCheck.position.y));

        Vector2 topLeft = new Vector2(touchDamageCheck.position.x - (touchDamageWidth / 2), touchDamageCheck.position.y + (touchDamageHeight / 2)); ;
        Vector2 topRight = new Vector2(touchDamageCheck.position.x + (touchDamageWidth / 2), touchDamageCheck.position.y + (touchDamageHeight / 2)); ;
        Vector2 bottomLeft = new Vector2(touchDamageCheck.position.x - (touchDamageWidth / 2), touchDamageCheck.position.y - (touchDamageHeight / 2));
        Vector2 bottomRight = new Vector2(touchDamageCheck.position.x + (touchDamageWidth / 2), touchDamageCheck.position.y - (touchDamageHeight / 2)); ;

        Gizmos.DrawLine(topLeft, bottomLeft);
        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);

    }
}
