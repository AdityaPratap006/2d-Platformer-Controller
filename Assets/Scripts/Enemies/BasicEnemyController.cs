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
        currentHealth -= attackDetails.damage;

        Instantiate(hitParticle, aliveGO.transform.position, Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)));

        if (attackDetails.positionX > aliveGO.transform.position.x)
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

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundCheck.position, new Vector2(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        Gizmos.DrawLine(wallCheck.position, new Vector2(wallCheck.position.x + wallCheckDistance, wallCheck.position.y));

    }
}
