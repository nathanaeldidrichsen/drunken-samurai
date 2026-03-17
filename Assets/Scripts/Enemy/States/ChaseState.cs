using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class ChaseState : State
{
    public ChaseState(EnemyBrain brain) : base(brain) { }

    public override void Update()
    {
        enemy.MoveTowardsPlayer();

        if (enemy.InAttackRange())
        {
            brain.ChangeState(brain.attack);
        }
    }
}