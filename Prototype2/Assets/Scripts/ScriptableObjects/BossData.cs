using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "bossData", menuName = "Boss Data", order = 1)]
public class BossData : ScriptableObject
{
    [Header("Base Stats")]
    public float health;
    [Range(0, 100)]
    public float resistance;
    public float meleeAttackRange;

    [Header("Weapon Stats")]
    public float weaponDamage;
    [Range(0.0f, 1.0f)]
    public float weaponAdrenalineModifier;
    public float tripleMaxCooldown;
    
    [Header("Projectile Stats")]
    public float projectileDamage;
    public float projectileForce;
    [Range(0.0f, 1.0f)]
    public float projectileAdrenalineModifier;

    [Header("AOE Stats")]
    public float aoeDamage;
    public float aoeRadius;
    public float aoeForce;
    public float aoeMaxCooldown;
    [Range(0.0f, 1.0f)]
    public float aoeModifier;

    [Header("Kick Stats")]
    public float kickDamage;
    public float kickForce;
    public float kickMaxCooldown;
    [Range(0.0f, 1.0f)]
    public float kickModifier;

    [Header("AI Stats")]
    [Tooltip("How long it takes in seconds for the boss to stop closing the distance.")]
    public float patience;

}
