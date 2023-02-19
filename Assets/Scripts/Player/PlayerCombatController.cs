using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{
    [SerializeField] bool combatEnabled;
    [SerializeField] float inputTimer;
    [SerializeField] float attack1Radius;
    [SerializeField] float attack1Damage;
    [SerializeField] float stunDamage = 1f;
    [SerializeField] Transform attack1HitBoxPos;
    [SerializeField] LayerMask whatIsDamageable;


    bool gotInput, isAttacking, isFirstAttack;
    float lastInputTime = Mathf.NegativeInfinity;
    AttackDetails attackDetails;

    Animator anim;
    PlayerController playerController;
    PlayerStats playerStats;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        playerStats = GetComponent<PlayerStats>();
        anim = GetComponent<Animator>();
        anim.SetBool("canAttack", combatEnabled);
    }

    private void Update()
    {
        CheckCombatInput();
        CheckAttacks();
    }

    private void CheckCombatInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (combatEnabled)
            {
                // Attempt Combat
                gotInput = true;
                lastInputTime = Time.time;
            }
        }
    }

    private void CheckAttacks()
    {
        if (gotInput)
        {
            // Perform attack 1
            if (!isAttacking)
            {
                gotInput = false;
                isAttacking = true;
                isFirstAttack = !isFirstAttack;
                anim.SetBool("attack1", true);
                anim.SetBool("firstAttack", isFirstAttack);
                anim.SetBool("isAttacking", isAttacking);
            }
        }

        if (Time.time >= lastInputTime + inputTimer)
        {
            // Wait for new input
            gotInput = false;
        }
    }

    private void CheckAttack1HitBox()
    {
        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(attack1HitBoxPos.position, attack1Radius, whatIsDamageable);

        attackDetails.damageAmount = attack1Damage;
        attackDetails.position = transform.position;
        attackDetails.stunDamageAmount = stunDamage;

        foreach (Collider2D collider in detectedObjects)
        {
            collider.transform.parent.SendMessage("Damage", attackDetails);
            // Instantiate hit particle
        }
    }

    private void FinishAttack1()
    {
        isAttacking = false;
        anim.SetBool("isAttacking", isAttacking);
        anim.SetBool("attack1", false);
    }

    private void Damage(AttackDetails attackDetails)
    {
        if (!playerController.GetDashStatus())
        {
            int direction;

            playerStats.DecreaseHealth(attackDetails.damageAmount);

            if (attackDetails.position.x < transform.position.x)
            {
                direction = 1;
            }
            else
            {
                direction = -1;
            }

            playerController.Knockback(direction);
        }
    }

    private void OnDrawGizmos()
    {
        // Gizmos.DrawWireSphere(attack1HitBoxPos.position, attack1Radius);
    }
}
