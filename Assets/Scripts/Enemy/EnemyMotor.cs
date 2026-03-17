using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * EnemyMotor
 * 
 * Handles the physical movement of the enemy.
 * 
 * Responsibilities:
 * - Moves the enemy toward a target or along a path.
 * - Stops movement for attacks, knockback, or other states.
 * - Optionally rotates or flips the sprite to face the player.
 * 
 * Connections:
 * - EnemyBase: uses speed, player reference, and movement helper methods.
 * - Rigidbody/Rigidbody2D: for physics-based movement.
 * - EnemyBrain: called from states to perform movement actions.
 * 
 * Usage:
 * - Call MoveTowardsPlayer(), StopMoving(), or custom movement methods from states.
 */
 
public class EnemyMotor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
