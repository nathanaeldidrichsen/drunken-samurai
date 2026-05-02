using System.Collections.Generic;

public enum BossStateId
{
    Idle,
    Run,
    PrepareSlam,
    Jump,
    Slam,
    SlamRecover,
    Spin
}

public class BossBrain
{
    private readonly Dictionary<BossStateId, BossState> states = new Dictionary<BossStateId, BossState>();

    public BossStateId CurrentStateId { get; private set; }
    public BossState CurrentState { get; private set; }

    public void RegisterState(BossStateId id, BossState state)
    {
        states[id] = state;
    }

    public void ChangeState(BossStateId id)
    {
        if (!states.TryGetValue(id, out BossState nextState))
            return;

        CurrentState?.Exit();
        CurrentState = nextState;
        CurrentStateId = id;
        CurrentState.Enter();
    }

    public void Update()
    {
        CurrentState?.Tick();
    }
}
