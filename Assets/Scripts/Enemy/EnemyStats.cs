using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Holds numeric configuration for an enemy.
// Purpose: Centralize tunable data (ranges, speeds, health) so multiple enemies can reuse or swap data.
// Connection: Read by EnemyBase and other systems (motor/combat/brain) to make decisions.
public class EnemyStats : MonoBehaviour
{
    [Header("Sensing / Movement")]
    public float detectRange = 6f;
    public float attackRange = 1.2f;
    public float moveSpeed = 2f;

    [Header("Attack Type")]
    public bool useRangedAttack = false;

    [Header("Combat / Health")]
    public int attackDamage = 1;
    public float attackCooldown = 0.8f;
    public float attackWindup = 0.2f;
    public int health = 3;

    [Header("Ranged")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileSpeed = 6f;
    public float projectileLifetime = 3f;
    public float shootInterval = 1.2f;
    public int projectileDamage = 1;
}
