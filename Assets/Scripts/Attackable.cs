using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RecoveryCounter))]
public class Attackable : MonoBehaviour
{
    [Header("Core")]
    public Stats stats;
    protected Animator anim;
    protected Rigidbody2D rb;
    protected RecoveryCounter recoveryCounter;

    [Header("State")]
    public bool isAlive = true;
    public bool isInvincible = false;
    public bool isKnockedBack = false;
    public bool isKnockable = false;
    // RequireComponent(typeof(SimpleFlash));
    private SimpleFlash simpleFlash;
    [Header("Audio")]
    public SoundData hurtSound;


    [Header("Feedback")]
    public float hurtSoundVolume = 0.4f;
    public float hitStopDuration = 0.05f;

    protected virtual void Awake()
    {
        simpleFlash = GetComponentInChildren<SimpleFlash>();
        recoveryCounter = GetComponent<RecoveryCounter>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (stats == null)
            stats = GetComponent<Stats>();
    }

    // --------------------
    // DAMAGE
    // --------------------
    public virtual void GetHurt(int damage)
    {
        if (!isAlive) return;
        if (recoveryCounter.recovering) return;
        simpleFlash?.Flash();

        recoveryCounter.counter = 0;
        if (!isInvincible)
        {
            stats.health -= damage;
        }

        if (anim != null)
            anim.SetTrigger("hurt");

        SoundManager.Instance?.PlaySFX(
hurtSound
        );

        StartCoroutine(HitStop(hitStopDuration));

        if (stats.health <= 0)
        {
            Die();
        }
    }

    // --------------------
    // HIT STOP
    // --------------------
    protected IEnumerator HitStop(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    // --------------------
    // KNOCKBACK
    // --------------------
    public virtual IEnumerator KnockBack(Vector2 direction, float force, float duration)
    {
        if (!isKnockable) yield break;
        if (isKnockedBack || rb == null) yield break;

        isKnockedBack = true;
        rb.velocity = Vector2.zero;

        float timer = 0f;

        while (timer < duration)
        {
            float t = timer / duration;
            float easedForce = Mathf.Lerp(force, 0f, t);

            rb.velocity = direction.normalized * easedForce;
            timer += Time.deltaTime;

            yield return null;
        }

        rb.velocity = Vector2.zero;
        isKnockedBack = false;
    }

    // --------------------
    // DEATH
    // --------------------
    protected virtual void Die()
    {
        if (!isAlive) return;
        if (isInvincible) return;


        isAlive = false;

        // Default behavior (can be overridden)
        Destroy(gameObject, 0.2f);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAlive) return;

        if (other.CompareTag("PlayerAttack"))
        {

            // PlayerStats stats = other.gameObject.GetComponentInParent<Player>().stats;
            // if (stats == null) return;
            GetHurt(1);

            // GetHurt(stats.damage);

            // Apply knockback if possible
            // if (rb != null && stats.knockbackForce > 0f)
            // {
            //     Vector2 dir = (transform.position - hitbox.owner.position).normalized;
            //     StartCoroutine(KnockBack(dir, hitbox.knockbackForce, hitbox.knockbackDuration));
            // }
        }
    }
}