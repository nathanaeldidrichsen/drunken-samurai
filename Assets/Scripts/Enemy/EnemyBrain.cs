using System;
using UnityEngine;


/*
 * EnemyBrain
 * 
 * The "mind" of the enemy. Handles the AI state machine and state transitions.
 * 
 * Responsibilities:
 * - Tracks the current state of the enemy (Idle, Chase, Attack, Hurt, Dead, etc.).
 * - Updates the current state each frame.
 * - Provides a method to change states and call Enter/Exit appropriately.
 * - Holds references to all possible states for this enemy.
 * 
 * Connections:
 * - EnemyBase: to access movement, health, and helper methods.
 * - States: IdleState, ChaseState, AttackState, HurtState, DeadState, etc.
 * 
 * Usage:
 * - Initialize with a reference to the EnemyBase.
 * - Call brain.Update() every frame in EnemyBase.
 */
 
public class EnemyBrain
{
    internal EnemyBase enemy;
    internal State currentState;

    internal IdleState idle;
    internal ChaseState chase;
    internal AttackState attack;
    internal HurtState hurt;
    internal DeadState dead;
    internal KnockbackState knockback;

    public EnemyBrain(EnemyBase enemy)
    {
        this.enemy = enemy;

        // create states
        idle = new IdleState(this);
        chase = new ChaseState(this);
        attack = new AttackState(this);
        hurt = new HurtState(this);
        dead = new DeadState(this);
        knockback = new KnockbackState(this);

        ChangeState(idle);
    }

    public void Update()
    {
        currentState?.Update();
    }

    internal void ChangeState(State newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    internal void ApplyKnockback()
    {
        enemy.ApplyKnockback();
    }
}