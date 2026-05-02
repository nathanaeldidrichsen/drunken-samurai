using UnityEngine;
using System.Collections;

public abstract class BossBase : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected Transform player;
    [SerializeField] protected Animator animator;
    [SerializeField] protected Rigidbody2D rb2D;
    [SerializeField] protected SpriteRenderer[] alwaysVisibleRenderers;

    [Header("Core Stats")]
    [SerializeField] protected int maxHealth = 300;
    [SerializeField] protected int contactDamage = 1;
    [SerializeField] protected float knockbackForce = 0.12f;

    [Header("Audio")]
    [SerializeField] protected SoundData hitSound;

    [Header("Hit Stop")]
    [SerializeField] protected float onHitStopDuration = 0.03f;
    private static bool hitStopActive;
    public static bool DebugHitStopActive => hitStopActive;
    public static int DebugHitStopTriggerCount { get; private set; }
    public static float DebugLastHitStopDuration { get; private set; }

    [Header("Movement")]
    [SerializeField] protected bool faceByScale = true;

    protected int currentHealth;
    protected BossBrain brain;
    protected SimpleFlash simpleFlash;
    protected SpriteRenderer[] spriteRenderers;

    public bool IsDead => currentHealth <= 0;

    protected virtual void Awake()
    {
        if (player == null && Player.Instance != null)
            player = Player.Instance.transform;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (rb2D == null)
            rb2D = GetComponent<Rigidbody2D>();

        simpleFlash = GetComponentInChildren<SimpleFlash>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        currentHealth = maxHealth;
        brain = new BossBrain();
        BuildStates(brain);
    }

    protected virtual void Update()
    {
        if (IsDead)
            return;

        if (player == null && Player.Instance != null)
            player = Player.Instance.transform;

        brain.Update();
    }

    protected abstract void BuildStates(BossBrain stateBrain);

    public virtual void TakeDamage(int damage, bool applyKnockback = false, float knockbackScale = 1f)
    {
        if (IsDead)
            return;

        simpleFlash?.Flash();
        PlaySfx(hitSound);
        // TriggerHitStop(onHitStopDuration);

        if (applyKnockback)
            ApplyKnockback(knockbackScale);

        currentHealth -= Mathf.Max(0, damage);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnBossDied();
        }
    }

    protected virtual void OnBossDied()
    {
        StopMoving();
        SetGraphicsVisible(true);
        PlayAnimation("dead");

        Collider2D[] colliders2D = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders2D)
            col.enabled = false;

        if (rb2D != null)
            rb2D.simulated = false;
    }

    protected void PlayAnimation(string stateName, float crossFade = 0.05f)
    {
        if (animator == null || string.IsNullOrWhiteSpace(stateName))
            return;

        animator.CrossFadeInFixedTime(stateName, crossFade);
    }

    protected void SetGraphicsVisible(bool visible)
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
            return;

        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            if (renderer != null)
                renderer.enabled = visible || IsAlwaysVisibleRenderer(renderer);
        }
    }

    private bool IsAlwaysVisibleRenderer(SpriteRenderer renderer)
    {
        if (alwaysVisibleRenderers == null || alwaysVisibleRenderers.Length == 0)
            return false;

        foreach (SpriteRenderer alwaysVisible in alwaysVisibleRenderers)
        {
            if (alwaysVisible == renderer)
                return true;
        }

        return false;
    }

    protected float DistanceToPlayer()
    {
        if (player == null)
            return float.MaxValue;

        return Vector2.Distance(transform.position, player.position);
    }

    protected void MoveTowards(Vector2 target, float speed)
    {
        Vector2 current = rb2D != null ? rb2D.position : (Vector2)transform.position;
        Vector2 next = Vector2.MoveTowards(current, target, Mathf.Max(0f, speed) * Time.deltaTime);

        if (rb2D != null)
            rb2D.MovePosition(next);
        else
            transform.position = next;

        if (faceByScale)
            FaceDirection(target.x - current.x);
    }

    protected void MoveTowardsPlayer(float speed)
    {
        if (player == null)
            return;

        MoveTowards(player.position, speed);
    }

    protected void StopMoving()
    {
        if (rb2D != null)
            rb2D.velocity = Vector2.zero;
    }

    protected void FaceDirection(float xDelta)
    {
        if (Mathf.Approximately(xDelta, 0f))
            return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (xDelta >= 0f ? 1f : -1f);
        transform.localScale = scale;
    }

    protected bool TryDamagePlayerInRadius(float radius, int damage)
    {
        if (Player.Instance == null)
            return false;

        float dist = Vector2.Distance(transform.position, Player.Instance.transform.position);
        if (dist > Mathf.Max(0f, radius))
            return false;

        Vector2 knockbackDir = ((Vector2)Player.Instance.transform.position - (Vector2)transform.position).normalized;
        Player.Instance.GetHurt(Mathf.Max(0, damage), knockbackDir);
        return true;
    }

    protected void ChangeState(BossStateId stateId)
    {
        brain.ChangeState(stateId);
    }

    protected void PlaySfx(SoundData sound)
    {
        if (sound == null)
            return;

        SoundManager.Instance?.PlaySFX(sound);
    }

    private void ApplyKnockback(float knockbackScale)
    {
        if (player == null)
            return;

        Vector2 direction = ((Vector2)transform.position - (Vector2)player.position).normalized;
        float force = Mathf.Max(0f, knockbackForce * Mathf.Max(0f, knockbackScale));

        if (rb2D != null)
            rb2D.AddForce(direction * force, ForceMode2D.Impulse);
        else
            transform.position += (Vector3)(direction * force);
    }

    protected void TriggerHitStop(float duration)
    {
        if (duration <= 0f || hitStopActive)
            return;

        DebugHitStopTriggerCount++;
        DebugLastHitStopDuration = duration;
        Debug.Log($"[BossBase] HitStop triggered. Duration={duration:0.000}s Count={DebugHitStopTriggerCount}");

        StartCoroutine(HitStopRoutine(duration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        hitStopActive = true;
        float previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(duration);

        if (Mathf.Approximately(Time.timeScale, 0f))
            Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;

        hitStopActive = false;
    }
}
