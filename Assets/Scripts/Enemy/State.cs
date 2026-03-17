using UnityEngine;

// Base abstract class for all enemy states.
// Purpose: Provide a common interface and store references to the EnemyBrain and EnemyBase.
// Connection: Each concrete state inherits this and uses the brain to request transitions and the enemy for actions.
public abstract class State
{
    protected EnemyBrain brain;
    protected EnemyBase enemy;

    public State(EnemyBrain brain)
    {
        this.brain = brain;
        this.enemy = brain.enemy;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}