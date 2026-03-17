using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * EnemyCombat
 * 
 * Handles the combat logic of the enemy, separate from movement and AI.
 * 
 * Responsibilities:
 * - Applies damage to the player.
 * - Calculates and applies knockback.
 * - Triggers attack animations and cooldowns.
 * - Manages interrupt logic (e.g., stagger or poise).
 * 
 * Connections:
 * - EnemyBase: to get attack ranges, speed, and helper methods.
 * - Player: to apply damage and knockback.
 * - EnemyBrain: can trigger state changes when combat events occur (e.g., Hurt, Knockback).
 * 
 * Usage:
 * - Optional: attach to enemy if combat is complex or has multiple attack types.
 */
 
public class EnemyCombat : MonoBehaviour
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
