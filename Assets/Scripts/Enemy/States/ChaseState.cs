using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class ChaseState : State
{
    public ChaseState(EnemyBrain brain) : base(brain) { }

    public override void Update()
    {
        if (enemy.InAttackRange())
        {
            brain.ChangeState(brain.attack);
            return;
        }

        enemy.MoveTowardsPlayer();
    }
}