using UnityEngine;

/*
 * EnemyBase
 * 
 * The central script for an enemy. This represents the "body" of the enemy.
 * 
 * Responsibilities:
 * - Holds core stats like health, speed, attack and detection ranges.
 * - Provides helper methods for movement, attacking, taking damage, and knockback.
 * - Plays animations (Hurt, Attack, Death).
 * - References the EnemyBrain for state management.
 * 
 * Connections:
 * - EnemyBrain: handles the current AI state and transitions.
 * - Player: target for chasing and attacking.
 * - Optional components: Rigidbody / Rigidbody2D, Animator, Collider/Collider2D.
 * 
 * Usage:
 * - Attach this to the enemy GameObject.
 * - Ensure Rigidbody or Rigidbody2D exists for movement and knockback.
 */

public class EnemyBase : MonoBehaviour
{
    [SerializeField] float dropForce = 3f;
    [SerializeField] float dropUpwardBias = 0.3f; // optional randomness
    public SoundData hurtSound;
    public SoundData dieSound;

    [SerializeField] GameObject[] itemsToDrop;

    public Transform player;
    // Delegate numerical configuration to EnemyStats component
    public EnemyStats stats;

    public EnemyBrain brain;

    void Awake()
    {
        // ensure an EnemyStats component exists on this GameObject
        stats = GetComponent<EnemyStats>() ?? gameObject.AddComponent<EnemyStats>();

        if (brain == null)
            brain = new EnemyBrain(this);
    }

    void Update()
    {
        brain.Update();
    }

    // Apply damage to this enemy. If `applyKnockback` is true the brain
    // will transition into the Knockback state, otherwise into Hurt (or Dead).
    // Other systems (player, projectiles) should call this method to damage the enemy.
    public void TakeDamage(int damage, bool applyKnockback = false)
    {
        SoundManager.Instance.PlaySFX(hurtSound);
        stats.health -= damage;

        if (stats.health <= 0)
        {
            ApplyKnockback();
            brain.ChangeState(brain.dead);
            SoundManager.Instance.PlaySFX(dieSound);

            return;
        }

        if (applyKnockback)
        {
            brain.ChangeState(brain.knockback);
            PlayHurtAnimation();
        }
        else
            brain.ChangeState(brain.hurt);
    }

    // --- helper methods used by states ---
    public bool PlayerInRange()
    {
        if (player == null || stats == null) return false;
        return Vector3.Distance(transform.position, player.position) <= stats.detectRange;
    }

    public bool InAttackRange()
    {
        if (player == null || stats == null) return false;
        return Vector3.Distance(transform.position, player.position) <= stats.attackRange;
    }

    public void MoveTowardsPlayer()
    {
        if (player == null || stats == null) return;
        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * stats.moveSpeed * Time.deltaTime;
        // optional: face player
        if (dir.x != 0) transform.localScale = new Vector3(Mathf.Sign(dir.x) * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    public void StopMoving()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.velocity = Vector3.zero;
        Rigidbody2D rb2 = GetComponent<Rigidbody2D>();
        if (rb2 != null) rb2.velocity = Vector2.zero;
    }

    public void PlayHurtAnimation()
    {
        Animator a = GetComponentInChildren<Animator>();
        if (a != null) a.SetTrigger("Hurt");
    }

    public void PlayAttackAnimation()
    {
        Animator a = GetComponentInChildren<Animator>();
        if (a != null) a.SetTrigger("Attack");
    }

    public void PlayDeathAnimation()
    {
        Animator a = GetComponentInChildren<Animator>();
        if (a != null) a.SetTrigger("Die");
    }

    public void DisableCollision()
    {
        Collider c = GetComponent<Collider>();
        if (c != null) c.enabled = false;
        Collider2D c2 = GetComponent<Collider2D>();
        if (c2 != null) c2.enabled = false;
    }

    public void ApplyKnockback()
    {
        if (player == null) return;
        Vector3 dir = (transform.position - player.position).normalized;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(dir * 0.1f, ForceMode.Impulse);
            return;
        }
        Rigidbody2D rb2 = GetComponent<Rigidbody2D>();
        if (rb2 != null)
        {
            rb2.AddForce((Vector2)dir * 0.1f, ForceMode2D.Impulse);
            return;
        }
        // fallback: nudge position
        transform.position += dir * 0.1f;
    }

    public void DropItem()
    {
        Debug.Log("Dropping items...");

        if (itemsToDrop == null || itemsToDrop.Length == 0)
            return;

        // 50% chance that nothing drops at all
        if (Random.value > 0.7f)
        {
            Debug.Log("No drop this time (50% chance)");
            return;
        }

        foreach (GameObject item in itemsToDrop)
        {
            // 40% drop chance per item
            float dropChance = 0.4f;

            if (Random.value <= dropChance)
            {
                SpawnDrop(item);
            }
        }
    }

    void SpawnDrop(GameObject itemDrop)
    {
        Vector3 spawnPos = transform.position + (Vector3)Random.insideUnitCircle * 0.2f;

        GameObject drop = Instantiate(itemDrop, spawnPos, Quaternion.identity);

        Rigidbody2D rb = drop.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;

            dir.y += dropUpwardBias;
            dir.Normalize();

            rb.AddForce(dir * dropForce, ForceMode2D.Impulse);
        }
    }


    void OnDrawGizmos()
    {
        if (stats != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, stats.detectRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, stats.attackRange);
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 6f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 1.2f);
        }
    }
}