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
    private bool attackAnimationFinished;
    private bool attackDamageAppliedThisCycle;

    void Awake()
    {
        // ensure an EnemyStats component exists on this GameObject
        stats = GetComponent<EnemyStats>() ?? gameObject.AddComponent<EnemyStats>();

        if (player == null && Player.Instance != null)
            player = Player.Instance.transform;

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

    public bool IsRangedEnemy()
    {
        return stats != null && stats.useRangedAttack;
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
        attackAnimationFinished = false;
        attackDamageAppliedThisCycle = false;
        Animator a = GetComponentInChildren<Animator>();
        if (a != null) a.SetTrigger("Attack");
    }

    // Animation event hook: call this from the attack clip at the hit frame.
    public void AnimationEvent_AttackHit()
    {
        TryDealAttackDamage();
    }

    // Animation event hook: call this on the final frame of the attack clip.
    public void AnimationEvent_AttackFinished()
    {
        attackAnimationFinished = true;
    }

    public bool HasAttackAnimationFinished()
    {
        return attackAnimationFinished;
    }

    public bool TryDealAttackDamage()
    {
        if (attackDamageAppliedThisCycle)
            return false;

        if (player == null && Player.Instance != null)
            player = Player.Instance.transform;

        if (!InAttackRange())
            return false;

        Player playerComponent = Player.Instance;
        if (playerComponent == null)
            return false;

        int damage = stats != null ? Mathf.Max(stats.attackDamage, 0) : 0;
        if (damage <= 0)
            return false;

        playerComponent.GetHurt(damage);
        attackDamageAppliedThisCycle = true;
        return true;
    }

    public bool TryShootProjectileAtPlayer()
    {
        if (stats == null || stats.projectilePrefab == null)
            return false;

        if (player == null && Player.Instance != null)
            player = Player.Instance.transform;

        if (player == null)
            return false;

        Transform spawnPoint = stats.projectileSpawnPoint != null ? stats.projectileSpawnPoint : transform;
        Vector2 direction = ((Vector2)player.position - (Vector2)spawnPoint.position).normalized;
        if (direction == Vector2.zero)
            direction = Vector2.right;

        Quaternion rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        GameObject projectileObj = Instantiate(stats.projectilePrefab, spawnPoint.position, rotation);

        EnemyProjectile projectile = projectileObj.GetComponent<EnemyProjectile>();
        if (projectile != null)
        {
            projectile.Initialize(
                direction,
                stats.projectileSpeed,
                Mathf.Max(stats.projectileDamage, 0),
                stats.projectileLifetime
            );
        }
        else
        {
            Rigidbody2D rb2 = projectileObj.GetComponent<Rigidbody2D>();
            if (rb2 != null)
                rb2.velocity = direction * stats.projectileSpeed;

            if (stats.projectileLifetime > 0f)
                Destroy(projectileObj, stats.projectileLifetime);
        }

        return true;
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