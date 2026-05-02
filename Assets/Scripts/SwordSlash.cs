using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordSlash : MonoBehaviour
{

public Transform playerTransform;
public float knockBackPower = .2f;
public float normalKnockbackScale = 0.25f;
public float finisherKnockbackScale = 1.0f;

void OnTriggerEnter2D(Collider2D other)
{
    if(other.CompareTag("Enemy"))
    {
        // other.GetComponent<Enemy>().GetHurt(this.gameObject.GetComponentInParent<Player>().stats.damage);
        // Vector2 attackDirection = other.transform.position - playerTransform.position;
        // StartCoroutine(other.GetComponent<Enemy>().KnockBack(attackDirection, knockBackPower, .3f));

        Player player = gameObject.GetComponentInParent<Player>();
        if (player == null)
            return;

        int damage = player.stats.damage;
        bool isFinisher = CombatController.Instance != null && CombatController.Instance.IsFinisherActive;
        bool applyKnockback = true;
        float knockbackScale = isFinisher ? finisherKnockbackScale : normalKnockbackScale;
        EnemyBase enemyBase = other.GetComponent<EnemyBase>();
        if (enemyBase != null)
        {
            enemyBase.TakeDamage(damage, applyKnockback, knockbackScale);
        }
        else
        {
            BossBase bossBase = other.GetComponent<BossBase>();
            if (bossBase != null)
                bossBase.TakeDamage(damage, applyKnockback, knockbackScale);
            else
                return;
        }

        Vector2 hitDirection = Vector2.right;
        if (playerTransform != null)
            hitDirection = ((Vector2)other.transform.position - (Vector2)playerTransform.position).normalized;

        if (player != null)
            player.ApplyAttackRecoil(-hitDirection);

        CameraShake.Instance?.ScreenShake(hitDirection, 1f);
        // Vector2 attackDirection = other.transform.position - playerTransform.position;
        // StartCoroutine(other.GetComponent<Enemy>().KnockBack(attackDirection, knockBackPower, .3f));

    }

    //     if(other.CompareTag("Attackable"))
    // {
    //     other.GetComponent<Attackable>().GetHurt(this.gameObject.GetComponentInParent<Player>().stats.damage);
    //     Vector2 attackDirection = other.transform.position - playerTransform.position;
    //     StartCoroutine(other.GetComponent<Attackable>().KnockBack(attackDirection, knockBackPower, .3f));

    // }
}

}
