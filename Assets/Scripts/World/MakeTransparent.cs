using UnityEngine;

public class MakeTransparent : MonoBehaviour
{
    private float transparencyAmount = 0.7f; // Amount of transparency (0 = fully transparent, 1 = opaque)
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        // Get the SpriteRenderer component attached to this GameObject
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Check if the colliding GameObject has the "Player" tag
        if (other.CompareTag("Player"))
        {
            // Set the alpha of the SpriteRenderer to the desired transparencyAmount
            Color color = spriteRenderer.color;
            color.a = transparencyAmount;
            spriteRenderer.color = color;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Reset the alpha of the SpriteRenderer when the player exits the trigger zone
        if (other.CompareTag("Player"))
        {
            Color color = spriteRenderer.color;
            color.a = 1f; // Reset alpha to fully opaque
            spriteRenderer.color = color;
        }
    }
}
