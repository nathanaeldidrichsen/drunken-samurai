using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

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

    private Action onDialogueEndCallback;

    public void StartDialogue(string[] dialogueLines, string npcName, Image image, Action onDialogueEnd = null)
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
            onDialogueEndCallback = onDialogueEnd;
            DisplayNextLine();
        }
    }

    public void DisplayNextLine()
    {
        if (continueButton == null) return;

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

    [SerializeField] private float hideAnimationDelay = 0.3f;

    public void EndDialogue(bool invokeCallback = true)
    {
        bool shouldInvoke = invokeCallback && onDialogueEndCallback != null;

        hasStartedConversation = false;
        hasFinishedConversation = true;
        isTyping = false;
        dialogueText.text = "";
        currentLineIndex = 0;
        dialogueAnim.SetTrigger("hide");

        if (shouldInvoke)
        {
            StartCoroutine(InvokeEndCallbackAfterDelay());
        }
        else
        {
            onDialogueEndCallback = null;
        }
    }

    private IEnumerator InvokeEndCallbackAfterDelay()
    {
        yield return new WaitForSeconds(hideAnimationDelay);
        onDialogueEndCallback?.Invoke();
        onDialogueEndCallback = null;
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
