using System.Collections;
using UnityEngine;

/// <summary>
/// Chest that can be opened once by the player pressing [E].
///
/// GO setup:
///   Chest (this script + Collider2D trigger + Rigidbody2D kinematic)
///   └─ GFX   (SpriteRenderer + Animator)  ← drag Animator into gfxAnimator
///   └─ Prompt (your [E] text object)       ← drag into promptTextObject
///
/// Spawn flow (wave clear):
///   Keep the chest GO *inactive* in the scene.
///   Add a GameEventListener on any GO → response calls chest.SetActive(true).
///   If useSpawnAnimation is ticked the spawn anim plays automatically when the GO wakes up.
/// </summary>
public class Chest : MonoBehaviour, IInteractable
{
    [Header("GFX")]
    [Tooltip("The Animator on the GFX child object.")]
    [SerializeField] private Animator gfxAnimator;

    [Header("Spawn Animation")]
    [Tooltip("Play a spawn animation when this chest first becomes active.")]
    [SerializeField] private bool useSpawnAnimation = false;
    [Tooltip("Animator trigger name for the spawn animation.")]
    [SerializeField] private string spawnAnimTrigger = "Spawn";

    [Header("Open Animation")]
    [Tooltip("Animator trigger name for the open animation.")]
    [SerializeField] private string openAnimTrigger = "Open";
    [Tooltip("Seconds to wait after triggering open anim before spawning items.")]
    [SerializeField] private float itemSpawnDelay = 0.5f;

    [Header("Items")]
    [Tooltip("Prefabs to instantiate when the chest opens.")]
    [SerializeField] private GameObject[] itemPrefabs;
    [Tooltip("Max random radius around the chest position where items land.")]
    [SerializeField] private float itemSpawnScatter = 0.35f;

    [Header("Interaction")]
    [Tooltip("The world-space UI object that shows the [E] prompt.")]
    [SerializeField] private GameObject promptTextObject;

    [Header("Audio")]
    [SerializeField] private SoundData openSound;

    // ── State ──────────────────────────────────────────────────────────────
    private bool playerIsClose = false;
    private bool isOpened     = false;

    // ── Unity Messages ─────────────────────────────────────────────────────

    private void Start()
    {
        // Hide prompt on start; it will show when the player enters range.
        if (promptTextObject != null)
            promptTextObject.SetActive(false);

        // Play spawn anim if ticked. Start() runs the first time the GO becomes
        // active, so this fires both at scene-load and when activated mid-game.
        if (useSpawnAnimation && gfxAnimator != null)
            gfxAnimator.SetTrigger(spawnAnimTrigger);
    }

    private void Update()
    {
        if (playerIsClose && !isOpened && Input.GetKeyDown(KeyCode.E))
            Interact(Player.Instance?.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isOpened && other.CompareTag("Player"))
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
        }
    }

    // ── IInteractable ──────────────────────────────────────────────────────

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor)) return;
        Open();
    }

    public bool CanInteract(GameObject interactor) => playerIsClose && !isOpened;

    public string GetInteractPrompt() => "[E] Open";

    // ── Public helpers ─────────────────────────────────────────────────────

    /// <summary>
    /// Call this from a GameEventListener response if you want to trigger the
    /// spawn animation manually (e.g. when re-enabling without SetActive).
    /// </summary>
    public void PlaySpawnAnimation()
    {
        if (useSpawnAnimation && gfxAnimator != null)
            gfxAnimator.SetTrigger(spawnAnimTrigger);
    }

    // ── Private ────────────────────────────────────────────────────────────

    private void Open()
    {
        isOpened = true;

        // Hide [E] prompt
        if (promptTextObject != null)
            promptTextObject.SetActive(false);

        // Sound
        if (openSound != null)
            SoundManager.Instance?.PlaySFX(openSound);

        // Open animation
        if (gfxAnimator != null)
            gfxAnimator.SetTrigger(openAnimTrigger);

        // Spawn items after a short delay so the animation has time to play
        StartCoroutine(SpawnItemsAfterDelay());
    }

    private IEnumerator SpawnItemsAfterDelay()
    {
        yield return new WaitForSeconds(itemSpawnDelay);
        SpawnItems();
    }

    private void SpawnItems()
    {
        if (itemPrefabs == null) return;

        foreach (var prefab in itemPrefabs)
        {
            if (prefab == null) continue;
            Vector2 scatter = Random.insideUnitCircle * itemSpawnScatter;
            Instantiate(prefab, (Vector2)transform.position + scatter, Quaternion.identity);
        }
    }

    // ── Editor Gizmos ──────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, itemSpawnScatter);
    }
}
