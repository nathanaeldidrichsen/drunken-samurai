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
    public float stuckComboTimeout = 0.9f;

    [Header("Debug")]
    public bool showDebugOverlay = false;
    public KeyCode toggleDebugKey = KeyCode.F3;

    // --- State ---
    [SerializeField] private int comboStep = 0;
    [SerializeField] private int bufferedClicks = 0;

    private bool canAcceptNextInput = false;
    private bool queuedFollowUp = false;
    private bool attackOnCooldown = false;

    private float comboTimer;
    private float attackCooldownTimer;
    private float stuckComboTimer;
    private float bufferedConsumeFlashTimer;
    private float failSafeFlashTimer;
    private int failSafeTriggerCount;
    private const float BufferedConsumeFlashDuration = 0.14f;
    private const float FailSafeFlashDuration = 0.5f;

    public bool IsAttacking => comboStep > 0;
    public bool IsFinisherActive => comboStep >= maxCombo && comboStep > 0;

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
        if (Input.GetKeyDown(toggleDebugKey))
            showDebugOverlay = !showDebugOverlay;

        // Attack cooldown
        if (attackOnCooldown)
        {
            attackCooldownTimer -= Time.deltaTime;
            if (attackCooldownTimer <= 0f)
                attackOnCooldown = false;
        }

        if (bufferedConsumeFlashTimer > 0f)
            bufferedConsumeFlashTimer -= Time.deltaTime;

        if (failSafeFlashTimer > 0f)
            failSafeFlashTimer -= Time.deltaTime;

        // Failsafe: if combo state stops receiving events, force recovery.
        if (comboStep > 0)
        {
            stuckComboTimer -= Time.deltaTime;
            if (stuckComboTimer <= 0f)
                ForceRecoverFromStuckCombo();
        }
        else
        {
            stuckComboTimer = 0f;
        }

        // Combo timeout is only active while the combo window is open.
        if (comboStep > 0 && canAcceptNextInput)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
            {
                StartCooldown(fullCombo: false);
                ResetCombo();
            }
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
        inputBufferTimer = inputBufferTime;

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
            bufferedConsumeFlashTimer = BufferedConsumeFlashDuration;
            comboStep = 1;
            canAcceptNextInput = false;
            queuedFollowUp = false;
            RefreshStuckComboTimer();

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
        bufferedConsumeFlashTimer = BufferedConsumeFlashDuration;
        comboStep++;
        canAcceptNextInput = false;
        queuedFollowUp = true;
        RefreshStuckComboTimer();

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
        comboTimer = comboWindow;
        RefreshStuckComboTimer();
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
        // A follow-up was queued from this animation, so skip end handling for
        // non-final steps only. If we are already on max combo, we must finalize.
        if (queuedFollowUp && comboStep < maxCombo)
        {
            queuedFollowUp = false;
            return;
        }

        queuedFollowUp = false;

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
        StartCooldown(fullCombo: false);
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

    private void RefreshStuckComboTimer()
    {
        if (comboStep <= 0) return;
        stuckComboTimer = stuckComboTimeout;
    }

    private void ForceRecoverFromStuckCombo()
    {
        failSafeTriggerCount++;
        failSafeFlashTimer = FailSafeFlashDuration;
        StartCooldown(fullCombo: false);
        ResetCombo();
    }

    private void ResetCombo()
    {
        comboStep = 0;
        bufferedClicks = 0;
        canAcceptNextInput = false;
        queuedFollowUp = false;
        comboTimer = 0f;
        stuckComboTimer = 0f;

        anim.SetInteger("ComboStep", 0);
        player.FreezePlayer(false);
    }

    private void OnGUI()
    {
        if (!showDebugOverlay) return;

        GUILayout.BeginArea(new Rect(14f, 14f, 320f, 220f), GUI.skin.box);
        GUILayout.Label("Combat Debug");
        GUILayout.Label("comboStep: " + comboStep + " / " + maxCombo);
        GUILayout.Label("bufferedClicks: " + bufferedClicks);

        Color previousColor = GUI.color;
        if (bufferedConsumeFlashTimer > 0f)
        {
            GUI.color = Color.green;
            GUILayout.Label("Buffered Input Consumed!");
        }
        else
        {
            GUI.color = Color.gray;
            GUILayout.Label("Buffered Input Waiting");
        }
        GUI.color = previousColor;

        GUILayout.Label("comboTimer: " + Mathf.Max(0f, comboTimer).ToString("0.000"));
        GUILayout.Label("inputBufferTimer: " + Mathf.Max(0f, inputBufferTimer).ToString("0.000"));
        GUILayout.Label("attackCooldownTimer: " + Mathf.Max(0f, attackCooldownTimer).ToString("0.000"));
        GUILayout.Label("stuckComboTimer: " + Mathf.Max(0f, stuckComboTimer).ToString("0.000"));
        GUILayout.Label("canAcceptNextInput: " + canAcceptNextInput);
        GUILayout.Label("queuedFollowUp: " + queuedFollowUp);
        GUILayout.Label("attackOnCooldown: " + attackOnCooldown);

        previousColor = GUI.color;
        if (failSafeFlashTimer > 0f)
        {
            GUI.color = Color.yellow;
            GUILayout.Label("Fail-safe recovery TRIGGERED");
        }
        else
        {
            GUI.color = Color.gray;
            GUILayout.Label("Fail-safe idle");
        }
        GUI.color = previousColor;
        GUILayout.Label("failSafeTriggerCount: " + failSafeTriggerCount);

        GUILayout.Space(8f);
        GUILayout.Label("HitStop Debug");
        GUILayout.Label("Enemy hitstop count: " + EnemyBase.DebugHitStopTriggerCount);
        GUILayout.Label("Enemy last duration: " + EnemyBase.DebugLastHitStopDuration.ToString("0.000"));
        GUILayout.Label("Enemy active: " + EnemyBase.DebugHitStopActive);
        GUILayout.Label("Boss hitstop count: " + BossBase.DebugHitStopTriggerCount);
        GUILayout.Label("Boss last duration: " + BossBase.DebugLastHitStopDuration.ToString("0.000"));
        GUILayout.Label("Boss active: " + BossBase.DebugHitStopActive);
        GUILayout.EndArea();
    }
}