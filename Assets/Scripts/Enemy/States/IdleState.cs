using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class IdleState : State
{
    public IdleState(EnemyBrain brain) : base(brain) { }

    public override void Update()
    {
        if (enemy.PlayerInRange())
        {
            brain.ChangeState(brain.chase);
        }
    }
}