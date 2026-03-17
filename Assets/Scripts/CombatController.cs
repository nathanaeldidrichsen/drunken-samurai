using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CombatController : MonoBehaviour
{
    public static CombatController Instance { get; private set; }

    [Header("References")]
    public Player player;
    public Animator anim;
    public Rigidbody2D rb;

    [Header("Attack Settings")]
    public float attackCooldown = 0.32f;
    public float fullComboCooldown = 1f;
    public float attackLungeForce = 0.015f;
    public float attackLungeDuration = 0.1f;
    public int maxCombo = 3;
    public float comboWindow = 0.35f;

    // --- State ---
    [SerializeField] private int comboStep = 0;
    [SerializeField] private int bufferedClicks = 0;

    private bool canAcceptNextInput = false;
    private bool attackOnCooldown = false;

    private float comboTimer;
    private float attackCooldownTimer;

    public bool IsAttacking => comboStep > 0;

    public float inputBufferTime = 0.25f; // maximum time a buffered tap waits
    private float inputBufferTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        if (!anim) anim = GetComponentInChildren<Animator>();
        if (!rb) rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Attack cooldown
        if (attackOnCooldown)
        {
            attackCooldownTimer -= Time.deltaTime;
            if (attackCooldownTimer <= 0f)
                attackOnCooldown = false;
        }

        // Combo timeout
        if (comboStep > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
                ResetCombo();
        }

        // Buffered input timeout (spam safety)
        if (bufferedClicks > 0)
        {
            inputBufferTimer -= Time.deltaTime;
            if (inputBufferTimer <= 0f)
                bufferedClicks = 0;
        }
    }

    // ======================
    // INPUT
    // ======================
    public void Attack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (attackOnCooldown) return;

        // Only queue up to remaining combo steps for this sequence.
        int maxBuffer = Mathf.Max(0, maxCombo - comboStep);
        if (maxBuffer <= 0) return;

        bufferedClicks = Mathf.Min(bufferedClicks + 1, maxBuffer);

        // Try immediately if we can transition now.
        TryConsumeBufferedInput();
    }

    // ======================
    // CORE COMBO LOGIC
    // ======================
    private void TryConsumeBufferedInput()
    {
        // FIRST ATTACK: always allowed
        if (comboStep == 0)
        {
            if (bufferedClicks <= 0)
                return;

            bufferedClicks--;
            comboStep = 1;
            comboTimer = comboWindow;
            canAcceptNextInput = false;

            HandleAttackDirection();
            anim.SetInteger("ComboStep", comboStep);
            anim.SetTrigger("Attack");

            player.FreezePlayer(true);
            return;
        }

        // FOLLOW-UP ATTACKS: need combo window
        if (!canAcceptNextInput) return;
        if (bufferedClicks <= 0) return;
        if (comboStep >= maxCombo) return;

        bufferedClicks--;
        comboStep++;

        comboTimer = comboWindow;
        canAcceptNextInput = false;

        HandleAttackDirection();
        anim.SetInteger("ComboStep", comboStep);
        anim.SetTrigger("Attack");

        player.FreezePlayer(true);
    }

    // ======================
    // ANIMATION EVENTS
    // ======================

    // Called ~50–60% into each attack animation
    public void EnableComboWindow()
    {
        canAcceptNextInput = true;
        TryConsumeBufferedInput(); // consume early clicks instantly
    }

    // Called at the lunge frame
    public void DoAttackLunge()
    {
        float multiplier = comboStep == maxCombo ? 1.6f : 1f;
        StartCoroutine(AttackLungeCoroutine(attackLungeForce * multiplier));
    }

    // Called at end of each attack animation
    public void EndAttack()
    {
        // If the combo is finished (max), finalize with longer cooldown and unfreeze.
        if (comboStep >= maxCombo)
        {
            StartCooldown(fullCombo: true);
            ResetCombo();
            player.FreezePlayer(false);
            return;
        }

        // If we still have buffered input, we will chain into next attack immediately.
        if (bufferedClicks > 0)
            return;

        // If we are in the combo window waiting for the next input, keep the player frozen
        // until the window expires (combo timer in Update will reset and unfreeze).
        if (canAcceptNextInput)
            return;

        // Otherwise, no pending combo input -> finish attack and allow movement.
        player.FreezePlayer(false);
        ResetCombo();
    }

    // ======================
    // HELPERS
    // ======================
    private IEnumerator AttackLungeCoroutine(float force)
    {
        Vector2 dir = new Vector2(
            anim.GetFloat("LastMoveX"),
            anim.GetFloat("LastMoveY")
        ).normalized;

        if (dir == Vector2.zero)
            dir = Vector2.down;

        rb.velocity = Vector2.zero;
        rb.AddForce(dir * force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(attackLungeDuration);

        rb.velocity = Vector2.zero;
    }

    private void HandleAttackDirection()
    {
        anim.SetInteger("AttackDirX", (int)anim.GetFloat("LastMoveX"));
        anim.SetInteger("AttackDirY", (int)anim.GetFloat("LastMoveY"));
    }

    private void StartCooldown(bool fullCombo = false)
    {
        attackOnCooldown = true;
        attackCooldownTimer = fullCombo ? fullComboCooldown : attackCooldown;
    }

    private void ResetCombo()
    {
        comboStep = 0;
        bufferedClicks = 0;
        canAcceptNextInput = false;

        anim.SetInteger("ComboStep", 0);
        player.FreezePlayer(false);
    }
}