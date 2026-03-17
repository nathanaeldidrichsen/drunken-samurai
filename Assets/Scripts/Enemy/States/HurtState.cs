using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class HurtState : State
{
    float hurtTime = 1f;

    public HurtState(EnemyBrain brain) : base(brain) { }

    public override void Enter()
    {
        enemy.StopMoving();
        enemy.PlayHurtAnimation();
    }

    public override void Update()
    {
        hurtTime -= Time.deltaTime;

        if (hurtTime <= 0)
        {
            brain.ChangeState(brain.chase);
        }
    }
}