using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordSlash : MonoBehaviour
{

public Transform playerTransform;
public float knockBackPower = .2f;

void OnTriggerEnter2D(Collider2D other)
{
    if(other.CompareTag("Enemy"))
    {
        // other.GetComponent<Enemy>().GetHurt(this.gameObject.GetComponentInParent<Player>().stats.damage);
        // Vector2 attackDirection = other.transform.position - playerTransform.position;
        // StartCoroutine(other.GetComponent<Enemy>().KnockBack(attackDirection, knockBackPower, .3f));


        other.GetComponent<EnemyBase>().TakeDamage(this.gameObject.GetComponentInParent<Player>().stats.damage, true);
        CameraShake.Instance.ScreenShake();
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
