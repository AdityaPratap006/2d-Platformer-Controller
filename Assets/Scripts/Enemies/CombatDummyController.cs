using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatDummyController : MonoBehaviour
{
    [SerializeField] float maxHealth;
    [SerializeField] bool applyKnockback;
    [SerializeField] Vector2 knockbackVelocity;
    [SerializeField] float knockbackDuration;
    [SerializeField] Vector2 knockbackDeathVelocity;
    [SerializeField] float deathTorque;
    [SerializeField] GameObject hitParticle;


    float currentHealth, knockbackStart;
    int playerFacingDirection;
    bool playerOnLeft;
    bool knockback;

    PlayerController playerController;
    GameObject aliveGO, brokenTopGO, brokenBottomGO;
    Rigidbody2D rbAlive, rbBrokenTop, rbBrokenBottom;
    Animator aliveAnim;

    private void Start()
    {
        currentHealth = maxHealth;

        playerController = GameObject.Find("Player").GetComponent<PlayerController>();

        aliveGO = transform.Find("Alive").gameObject;
        brokenTopGO = transform.Find("BrokenTop").gameObject;
        brokenBottomGO = transform.Find("BrokenBottom").gameObject;

        rbAlive = aliveGO.GetComponent<Rigidbody2D>();
        rbBrokenTop = brokenTopGO.GetComponent<Rigidbody2D>();
        rbBrokenBottom = brokenBottomGO.GetComponent<Rigidbody2D>();
        aliveAnim = aliveGO.GetComponent<Animator>();

        aliveGO.SetActive(true);
        brokenTopGO.SetActive(false);
        brokenBottomGO.SetActive(false);
    }

    private void Update()
    {
        CheckKnockback();
    }

    private void Damage(AttackDetails attackDetails)
    {
        currentHealth -= attackDetails.damageAmount;

        if (attackDetails.position.x < aliveGO.transform.position.x)
        {
            playerFacingDirection = 1;
        }
        else
        {
            playerFacingDirection = -1;
        }

        Instantiate(hitParticle, aliveGO.transform.position, Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)));

        if (playerFacingDirection == 1)
        {
            playerOnLeft = true;
        }
        else
        {
            playerOnLeft = false;
        }

        aliveAnim.SetBool("playerOnLeft", playerOnLeft);
        aliveAnim.SetTrigger("damage");

        if (applyKnockback && currentHealth > 0.0f)
        {
            Knockback();
        }

        if (currentHealth <= 0.0f)
        {
            Die();
        }
    }

    private void Knockback()
    {
        knockback = true;
        knockbackStart = Time.time;
        rbAlive.velocity = new Vector2(knockbackVelocity.x * playerFacingDirection, knockbackVelocity.y);
    }

    private void CheckKnockback()
    {
        if ((Time.time >= knockbackStart + knockbackDuration) && knockback)
        {
            knockback = false;
            rbAlive.velocity = new Vector2(0.0f, rbAlive.velocity.y);
        }
    }

    private void Die()
    {
        aliveGO.SetActive(false);
        brokenTopGO.SetActive(true);
        brokenBottomGO.SetActive(true);

        brokenTopGO.transform.position = aliveGO.transform.position;
        brokenBottomGO.transform.position = aliveGO.transform.position;

        rbBrokenBottom.velocity = new Vector2(knockbackVelocity.x * playerFacingDirection, knockbackVelocity.y);
        rbBrokenTop.velocity = new Vector2(knockbackDeathVelocity.x * playerFacingDirection, knockbackDeathVelocity.y);
        rbBrokenTop.AddTorque(deathTorque * -playerFacingDirection, ForceMode2D.Impulse);
    }
}
