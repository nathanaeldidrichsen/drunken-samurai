public abstract class BossState
{
    protected readonly BossBase boss;
    protected readonly BossBrain brain;

    protected BossState(BossBase boss, BossBrain brain)
    {
        this.boss = boss;
        this.brain = brain;
    }

    public virtual void Enter() { }
    public virtual void Tick() { }
    public virtual void Exit() { }
}
