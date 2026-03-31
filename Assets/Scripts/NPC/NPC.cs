using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class NPC : MonoBehaviour, IInteractable
{
    public string[] dialogue;
    public DialogueManager dialogueManager;
    public Image dialogueImage;
    public UnityEvent onDialogueEnd;
    public string npcName;
    private bool playerIsClose = false;
    [SerializeField] private Patrolling patrolling;
    public bool patrol;
    [Header("Interaction")]
    public GameObject promptTextObject;

    void Start()
    {
        if (GetComponent<Patrolling>() != null && patrol)
        {
            patrolling = GetComponent<Patrolling>();
        }

        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsClose = true;
            if (promptTextObject != null)
                promptTextObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsClose = false;
            if (promptTextObject != null)
                promptTextObject.SetActive(false);
            dialogueManager.EndDialogue(false);
        }
    }

    private void Update()
    {
        if (promptTextObject != null)
        {
            promptTextObject.SetActive(playerIsClose);
        }

        if(patrolling != null && dialogueManager.hasStartedConversation)
        {
            patrolling.isTalking = true;
        }



        if (playerIsClose && Input.GetKeyDown(KeyCode.E))
        {
            if (!dialogueManager.hasStartedConversation || dialogueManager.hasFinishedConversation)
            {
                StartDialogue();
            }
            else if (!dialogueManager.isTyping)
            {
                dialogueManager.DisplayNextLine();
            }
        }

        // Check if NPC is in dialogue
        if (patrolling != null && dialogueManager.hasStartedConversation)
        {
            patrolling.StopPatrol();
        }

        if (patrolling != null && !dialogueManager.hasStartedConversation || patrolling != null && dialogueManager.hasFinishedConversation)
        {
            patrolling.StartPatrol();
        }
    }

    private void StartDialogue()
    {
        dialogueManager.StartDialogue(dialogue, npcName, dialogueImage, OnDialogueEnd);
    }

    private void OnDialogueEnd()
    {
        onDialogueEnd?.Invoke();
    }

    public void Interact(GameObject interactor)
    {
        if (playerIsClose)
        {
            if (!dialogueManager.hasStartedConversation || dialogueManager.hasFinishedConversation)
            {
                StartDialogue();
            }
            else if (!dialogueManager.isTyping)
            {
                dialogueManager.DisplayNextLine();
            }
        }
    }

    public bool CanInteract(GameObject interactor)
    {
        return playerIsClose;
    }

    public string GetInteractPrompt()
    {
        return "[E] Talk";
    }
}
