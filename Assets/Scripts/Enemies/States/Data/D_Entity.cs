using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newEntityData", menuName = "Data/Entity Data/Base Data")]
public class D_Entity : ScriptableObject
{
    [Header("Health/Damage Attributes")]
    public float maxHealth = 40f;
    public float damageHopSpeed = 3f;
    public float stunResistance = 3f;
    public float stunRecoveryTime = 2f;
    public GameObject hitParticle;


    [Header("Ground Detection Attributes")]
    public float groundCheckRadius = 0.3f;


    [Header("Wall/Ledge Detection Attributes")]
    public float wallCheckDistance = 0.2f;
    public float ledgeCheckDistance = 0.4f;
    public LayerMask whatIsGround;


    [Header("Player Detection Attributes")]
    public float minAgroDistance = 3f;
    public float maxAgroDistance = 5f;
    public LayerMask whatIsPlayer;


    [Header("Attack Action Attributes")]
    public float closeRangeActionDistance = 1f;

}
