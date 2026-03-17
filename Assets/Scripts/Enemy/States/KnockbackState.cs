using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class KnockbackState : State
{
    float time;
    float duration = 1f;

    public KnockbackState(EnemyBrain brain) : base(brain) { }

    public override void Enter()
    {
        time = duration; // RESET TIMER
        enemy.ApplyKnockback();
    }

    public override void Update()
    {
        time -= Time.deltaTime;

        if (time <= 0)
        {
            enemy.StopMoving(); // STOP VELOCITY
            brain.ChangeState(brain.chase);

        }
    }
}