using UnityEngine;

public class Boss1 : BossBase
{
    [Header("Boss 1 Pattern")]
    [SerializeField] private float runSpeed = 2.4f;
    [SerializeField] private float startComboDistance = 3f;

    [SerializeField] private float idleDuration = 1f;
    [SerializeField] private float prepareSlamDuration = 0.6f;
    [SerializeField] private float jumpSpeed = 7f;
    [SerializeField] private float jumpArrivalDistance = 0.1f;
    [SerializeField] private float jumpVanishDelay = 0.12f;

    [SerializeField] private float slamDuration = 0.35f;
    [SerializeField] private int slamDamage = 2;
    [SerializeField] private float slamDamageRadius = 1.4f;
    [SerializeField] private float slamShakeIntensity = 1.2f;

    [SerializeField] private float slamRecoverDuration = 2f;

    [SerializeField] private float spinDuration = 4f;
    [SerializeField] private float spinMoveSpeed = 3f;
    [SerializeField] private int spinTickDamage = 1;
    [SerializeField] private float spinTickDamageRadius = 1f;
    [SerializeField] private float spinTickInterval = 0.35f;

    [Header("Audio")]
    [SerializeField] private SoundData prepareSlamSound;
    [SerializeField] private SoundData jumpSound;
    [SerializeField] private SoundData slamSound;
    [SerializeField] private SoundData spinSound;

    [Header("Animation State Names")]
    [SerializeField] private string idleAnim = "idle";
    [SerializeField] private string runAnim = "run";
    [SerializeField] private string prepareSlamAnim = "prepare_slam";
    [SerializeField] private string jumpAnim = "jump";
    [SerializeField] private string slamAnim = "slam";
    [SerializeField] private string slamRecoverAnim = "slam_recover";
    [SerializeField] private string spinAnim = "spin";

    private Vector2 jumpTarget;

    protected override void BuildStates(BossBrain stateBrain)
    {
        stateBrain.RegisterState(BossStateId.Run, new RunState(this, stateBrain));
        stateBrain.RegisterState(BossStateId.Idle, new IdleState(this, stateBrain));
        stateBrain.RegisterState(BossStateId.PrepareSlam, new PrepareSlamState(this, stateBrain));
        stateBrain.RegisterState(BossStateId.Jump, new JumpState(this, stateBrain));
        stateBrain.RegisterState(BossStateId.Slam, new SlamState(this, stateBrain));
        stateBrain.RegisterState(BossStateId.SlamRecover, new SlamRecoverState(this, stateBrain));
        stateBrain.RegisterState(BossStateId.Spin, new SpinState(this, stateBrain));

        stateBrain.ChangeState(BossStateId.Run);
    }

    protected override void OnBossDied()
    {
        base.OnBossDied();
        Destroy(gameObject, 1.25f);
    }

    private class RunState : BossState
    {
        private readonly Boss1 owner;

        public RunState(Boss1 boss, BossBrain brain) : base(boss, brain)
        {
            owner = boss;
        }

        public override void Enter()
        {
            owner.PlayAnimation(owner.runAnim);
        }

        public override void Tick()
        {
            owner.MoveTowardsPlayer(owner.runSpeed);

            if (owner.DistanceToPlayer() <= owner.startComboDistance)
            {
                owner.ChangeState(BossStateId.Idle);
            }
        }
    }

    private class IdleState : BossState
    {
        private readonly Boss1 owner;
        private float timer;

        public IdleState(Boss1 boss, BossBrain brain) : base(boss, brain)
        {
            owner = boss;
        }

        public override void Enter()
        {
            timer = owner.idleDuration;
            owner.StopMoving();
            owner.PlayAnimation(owner.idleAnim);
        }

        public override void Tick()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
                owner.ChangeState(BossStateId.PrepareSlam);
        }
    }

    private class PrepareSlamState : BossState
    {
        private readonly Boss1 owner;
        private float timer;

        public PrepareSlamState(Boss1 boss, BossBrain brain) : base(boss, brain)
        {
            owner = boss;
        }

        public override void Enter()
        {
            timer = owner.prepareSlamDuration;
            owner.StopMoving();
            owner.PlayAnimation(owner.prepareSlamAnim);
            owner.PlaySfx(owner.prepareSlamSound);
        }

        public override void Tick()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                owner.jumpTarget = owner.player != null ? (Vector2)owner.player.position : (Vector2)owner.transform.position;
                owner.ChangeState(BossStateId.Jump);
            }
        }
    }

    private class JumpState : BossState
    {
        private readonly Boss1 owner;
        private float vanishTimer;
        private bool isHidden;

        public JumpState(Boss1 boss, BossBrain brain) : base(boss, brain)
        {
            owner = boss;
        }

        public override void Enter()
        {
            owner.SetGraphicsVisible(true);
            owner.PlayAnimation(owner.jumpAnim);
            owner.PlaySfx(owner.jumpSound);
            vanishTimer = Mathf.Max(0f, owner.jumpVanishDelay);
            isHidden = false;
        }

        public override void Tick()
        {
            if (!isHidden)
            {
                vanishTimer -= Time.deltaTime;
                if (vanishTimer <= 0f)
                {
                    owner.SetGraphicsVisible(false);
                    isHidden = true;
                }
            }

            owner.MoveTowards(owner.jumpTarget, owner.jumpSpeed);

            float dist = Vector2.Distance(owner.transform.position, owner.jumpTarget);
            if (dist <= owner.jumpArrivalDistance)
            {
                owner.ChangeState(BossStateId.Slam);
            }
        }
    }

    private class SlamState : BossState
    {
        private readonly Boss1 owner;
        private float timer;
        private bool damageApplied;

        public SlamState(Boss1 boss, BossBrain brain) : base(boss, brain)
        {
            owner = boss;
        }

        public override void Enter()
        {
            timer = owner.slamDuration;
            damageApplied = false;
            owner.StopMoving();
            owner.SetGraphicsVisible(true);
            owner.PlayAnimation(owner.slamAnim);
            owner.PlaySfx(owner.slamSound);

            Vector2 shakeDirection = Vector2.zero;
            if (owner.player != null)
                shakeDirection = ((Vector2)owner.player.position - (Vector2)owner.transform.position).normalized;
            CameraShake.Instance?.ScreenShake(shakeDirection, owner.slamShakeIntensity);
        }

        public override void Tick()
        {
            if (!damageApplied)
            {
                owner.TryDamagePlayerInRadius(owner.slamDamageRadius, owner.slamDamage);
                damageApplied = true;
            }

            timer -= Time.deltaTime;
            if (timer <= 0f)
                owner.ChangeState(BossStateId.SlamRecover);
        }
    }

    private class SlamRecoverState : BossState
    {
        private readonly Boss1 owner;
        private float timer;

        public SlamRecoverState(Boss1 boss, BossBrain brain) : base(boss, brain)
        {
            owner = boss;
        }

        public override void Enter()
        {
            timer = owner.slamRecoverDuration;
            owner.StopMoving();
            owner.PlayAnimation(owner.slamRecoverAnim);
        }

        public override void Tick()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
                owner.ChangeState(BossStateId.Spin);
        }
    }

    private class SpinState : BossState
    {
        private readonly Boss1 owner;
        private float timer;
        private float damageTickTimer;

        public SpinState(Boss1 boss, BossBrain brain) : base(boss, brain)
        {
            owner = boss;
        }

        public override void Enter()
        {
            timer = owner.spinDuration;
            damageTickTimer = 0f;
            owner.PlayAnimation(owner.spinAnim);
            owner.PlaySfx(owner.spinSound);
        }

        public override void Tick()
        {
            timer -= Time.deltaTime;
            damageTickTimer -= Time.deltaTime;

            owner.MoveTowardsPlayer(owner.spinMoveSpeed);

            if (damageTickTimer <= 0f)
            {
                owner.TryDamagePlayerInRadius(owner.spinTickDamageRadius, owner.spinTickDamage);
                damageTickTimer = Mathf.Max(0.05f, owner.spinTickInterval);
            }

            if (timer <= 0f)
                owner.ChangeState(BossStateId.Run);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, startComboDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, slamDamageRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, spinTickDamageRadius);
    }
}
