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

    [Header("Combat / Health")]
    public int health = 3;
}
