using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AttackState : State
{
    float attackTimer;

    public AttackState(EnemyBrain brain) : base(brain) { }

    public override void Enter()
    {
        attackTimer = 0.5f;
        enemy.StopMoving();
        enemy.PlayAttackAnimation();
    }

    public override void Update()
    {
        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0)
        {
            brain.ChangeState(brain.chase);
        }
    }
}