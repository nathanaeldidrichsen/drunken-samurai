using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;
    public GameObject dialoguePanel;
    public GameObject continueButton;
    [SerializeField] private Image dialogueImageIcon;
    public Animator dialogueAnim;
    public float wordSpeed;
    public bool hasFinishedConversation;
    public bool isTyping;
    public bool hasStartedConversation;
        [Header("Audio")]
    public SoundData npcStartAndFinishSound;
    public SoundData npcTalkingSound;



    private string[] currentDialogueLines;
    public int currentLineIndex = 0;

    public void StartDialogue(string[] dialogueLines, string npcName, Image image)
    {
        if (!hasStartedConversation)
        {
            nameText.text = npcName.ToString();
            SoundManager.Instance.PlaySFX(npcStartAndFinishSound);
            dialogueText.text = "";
            hasStartedConversation = true;
            dialogueImageIcon = image;
            hasFinishedConversation = false;
            dialogueAnim.SetTrigger("show");
            currentDialogueLines = dialogueLines;
            currentLineIndex = 0;
            DisplayNextLine();
        }
    }

    public void DisplayNextLine()
    {
            SoundManager.Instance.PlaySFX(npcTalkingSound);


        continueButton.SetActive(false);
        if (currentLineIndex >= currentDialogueLines.Length - 1)
        {
            hasFinishedConversation = true;
        }
        if (currentLineIndex < currentDialogueLines.Length - 1)
        {
            dialogueText.text = "";
            currentLineIndex++;
            StartCoroutine(Typing());
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        hasStartedConversation = false;
        hasFinishedConversation = true;
        isTyping = false;
        dialogueText.text = "";
        currentLineIndex = 0;
        dialogueAnim.SetTrigger("hide");
    }

    public IEnumerator Typing()
    {
        foreach (char letter in currentDialogueLines[currentLineIndex].ToCharArray())
        {
            SoundManager.Instance.PlaySFX(npcTalkingSound);

            dialogueText.text += letter;
            isTyping = true;
            if (dialogueText.text == currentDialogueLines[currentLineIndex])
            {
                isTyping = false;

                if (currentLineIndex == currentDialogueLines.Length - 1)
                {
                    continueButton.SetActive(false);
                }
                else
                {
                    continueButton.SetActive(true);
                }
            }
            yield return new WaitForSeconds(wordSpeed);
        }
    }
}
