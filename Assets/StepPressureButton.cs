using UnityEngine;

public class StepPressureButton : MonoBehaviour
{
    [Tooltip("GameEvent to raise when the button is triggered.")]
    public GameEvent onTriggered;

    [Tooltip("Sound to play when the button is triggered.")]
    public SoundData triggerSound;

    [Tooltip("If true, the event fires every time the player steps on it. If false, only once.")]
    public bool repeatEvent = false;

    private Animator animator;
    private bool hasTriggered = false;
    private int playerCount = 0;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerCount++;
        SetTriggered(true);

        if (repeatEvent || !hasTriggered)
        {
            hasTriggered = true;
            SoundManager.Instance?.PlaySFX(triggerSound);
            onTriggered?.Raise();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerCount--;
        if (playerCount <= 0)
        {
            playerCount = 0;
            SetTriggered(false);
        }
    }

    private void SetTriggered(bool value)
    {
        if (animator != null)
            animator.SetBool("isTriggered", value);
    }
}

