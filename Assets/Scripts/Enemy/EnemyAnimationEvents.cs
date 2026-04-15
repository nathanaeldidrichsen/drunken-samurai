using UnityEngine;

// Put this on the same GameObject as the Animator that plays the enemy clips.
public class EnemyAnimationEvents : MonoBehaviour
{
    [SerializeField] private EnemyBase enemy;

    private void Awake()
    {
        if (enemy == null)
            enemy = GetComponentInParent<EnemyBase>();
    }

    public void AnimationEvent_AttackHit()
    {
        if (enemy != null)
            enemy.TryDealAttackDamage();
    }

    public void AnimationEvent_AttackFinished()
    {
        if (enemy != null)
            enemy.AnimationEvent_AttackFinished();
    }
}
