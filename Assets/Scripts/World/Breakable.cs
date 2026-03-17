using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{

    public int health;
    private RecoveryCounter recoveryCounter;
    [SerializeField] private GameObject breakParticle;
    [SerializeField] private GameObject dropItem;

    [Header("Audio")]
    public SoundData breakSound;

    // Start is called before the first frame update
    void Start()
    {
        if (recoveryCounter == null)
        {
            recoveryCounter = GetComponent<RecoveryCounter>();
        }
    }

    public void TakeDamage()
    {
        if (health > 0 && !recoveryCounter.recovering)
        {
            health--;
            recoveryCounter.counter = 0;

            if (health <= 0)
            {
                DestroyObject();
            }
        }

        if (health <= 0)
        {
            DestroyObject();

        }
    }

    public void DestroyObject()
    {
        SoundManager.Instance.PlaySFX(breakSound);
        if (breakParticle != null)
        {
            GameObject particleObject = Instantiate(breakParticle, transform.position, Quaternion.identity);



            if (this.gameObject.transform.parent != null)
            {
                particleObject.transform.SetParent(this.gameObject.transform.parent);

                if (dropItem != null)
                {
                    GameObject dropItemObject = Instantiate(dropItem, transform.position, Quaternion.identity);
                    dropItemObject.transform.SetParent(this.gameObject.transform.parent);
                }
            }
        }
        Destroy(this.gameObject);
        // play sound and particle
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            TakeDamage();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("PlayerAttack"))
        {
            TakeDamage();
        }
    }
}
