using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class DeadState : State
{
    public DeadState(EnemyBrain brain) : base(brain) { }

    public override void Enter()
    {
        enemy.PlayDeathAnimation();
        enemy.DropItem();
        enemy.DisableCollision();
        enemy.NotifyWaveDied();
    }
}