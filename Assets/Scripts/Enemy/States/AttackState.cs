using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AttackState : State
{
    float fallbackExitTimer;
    float shootTimer;

    public AttackState(EnemyBrain brain) : base(brain) { }

    public override void Enter()
    {
        if (enemy.IsRangedEnemy())
        {
            shootTimer = 0f;
            enemy.StopMoving();
            return;
        }

        float cooldown = (enemy.stats != null) ? enemy.stats.attackCooldown : 0.5f;
        fallbackExitTimer = Mathf.Max(cooldown + 0.2f, 0.25f);
        enemy.StopMoving();
        enemy.PlayAttackAnimation();
    }

    public override void Update()
    {
        if (enemy.IsRangedEnemy())
        {
            if (!enemy.InAttackRange())
            {
                brain.ChangeState(brain.chase);
                return;
            }

            enemy.StopMoving();
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0f)
            {
                enemy.TryShootProjectileAtPlayer();
                float interval = (enemy.stats != null) ? enemy.stats.shootInterval : 1f;
                shootTimer = Mathf.Max(interval, 0.05f);
            }
            return;
        }

        enemy.StopMoving();

        fallbackExitTimer -= Time.deltaTime;

        if (enemy.HasAttackAnimationFinished() || fallbackExitTimer <= 0f)
        {
            brain.ChangeState(brain.chase);
        }
    }
}