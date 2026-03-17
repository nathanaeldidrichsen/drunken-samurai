using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickupable : MonoBehaviour
{
    private Rigidbody2D rb;
    private float pickUpRadius = 0.4f;
    private Transform playerTransform;
    public bool isInventoryItem;
    private Consumable consumable;
    [SerializeField] private Item item;
    public int itemStackAmount;

    [Header("Audio")]
    public SoundData gemSound;
    public SoundData flaskSound;

    
    void Start()
    {
        if(!isInventoryItem)
        {
            consumable = gameObject.GetComponent<Consumable>();
        }
        rb = GetComponent<Rigidbody2D>();
        // pickUpRadius = Player.Instance.itemPickUpRadius; // Make sure your Player class has a public float field named pickUpRadius
        playerTransform = Player.Instance.transform; // Ensure your Player class has a Transform property or field accessible here
    }
    
    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if(distanceToPlayer <= pickUpRadius)
        {
            MoveTowardsPlayer();
        }
    }

    void MoveTowardsPlayer()
    {
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        rb.MovePosition(rb.position + directionToPlayer * Time.fixedDeltaTime * 0.3f); // You may want to multiply this by a speed variable
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isInventoryItem)
            {
                SoundManager.Instance.PlaySFX(gemSound);
                Player.Instance.GainExp(consumable.expAmount);
                Player.Instance.GainHealth(consumable.healAmount);
                Player.Instance.GainGold(consumable.goldAmount);
                // if (consumable.expAmount > 0)
                // {
                // }
                Destroy(gameObject); // Destroys this pickupable item

                // if (consumable.healAmount > 0)
                // {
                //     SoundManager.Instance.PlaySFX(flaskSound);
                // Destroy(gameObject); // Destroys this pickupable item
                // }
            }
            else
            {
                Inventory.Instance.AddItem(item, itemStackAmount);
                SoundManager.Instance.PlaySFX(gemSound);
                Destroy(gameObject); // Destroys this pickupable item
            }


        }
    }

    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.blue; // Set the color of the gizmo

    //     // Draw a wire sphere representing the pickup radius
    //     Gizmos.DrawWireSphere(transform.position, pickUpRadius);
    // }
}
