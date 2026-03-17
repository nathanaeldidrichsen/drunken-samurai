using UnityEngine;

public class Forge : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    public float interactRadius = 2f;
    public bool playerIsClose = false;
    public GameObject promptTextObject;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerIsClose = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerIsClose = false;
    }

    private void Update()
    {
        // Show/hide [E] prompt object based on proximity
        if (promptTextObject != null)
        {
            promptTextObject.SetActive(playerIsClose);
        }

        if (playerIsClose && Input.GetKeyDown(KeyCode.E))
        {
            Interact(GameObject.FindGameObjectWithTag("Player"));
        }
    }

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor))
            return;

        if (HUD.Instance != null)
        {
            HUD.Instance.OpenForge();
        }
        else
        {
            Debug.LogWarning("HUD.Instance is null, cannot open forge.");
        }
    }

    public bool CanInteract(GameObject interactor)
    {
        return playerIsClose;
    }

    public string GetInteractPrompt()
    {
        return "[E] Open Forge";
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
