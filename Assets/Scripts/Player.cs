using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [SerializeField] public PlayerStats stats;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private int health = 100;
    [SerializeField] private int defense = 0;

    [SerializeField] private float dashSpeed = 1f;
    [SerializeField] private float dashCooldown = 2f; // Seconds
    [SerializeField] private float rollCooldown = 2f; // Seconds
    [SerializeField] private float attackCoolDown = 1f; // Seconds


    public bool isFrozen = false;
    public float enemySpawnRadius = 10f;
    [HideInInspector] public float itemPickUpRadius = 0.2f;
    private RecoveryCounter recoveryCounter;

    // PS4 controls
    public float horizontal;
    public float vertical;
    public bool isRolling;
    private float rollForce = .04f;

    // --- Combo system ---

    private int wellHealCost = 5;

    [Header("Audio")]
    public SoundData levelUpSound;
    public SoundData hurtSound;
    public SoundData wellHealSound;

    [Header("VFX")]
    public ParticleSystem healParticle;

    public Animator anim;
    private SpriteRenderer sprite;
    private Rigidbody2D rb;
    private UnityEngine.Vector2 movement;
    public float currentCooldown;
    private UnityEngine.Vector2 targetPosition;
    private SimpleFlash simpleFlash;
    // private bool isDashing = false;
    private CapsuleCollider2D col;
    private static Player instance;
    [SerializeField] PlayerPosition playerPos;
    public static Player Instance
    {
        get
        {
            if (instance == null) instance = GameObject.FindObjectOfType<Player>();
            return instance;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (playerPos != null)
        {
            transform.position = new Vector2(playerPos.x, playerPos.y);
        }

        //ResetStats();
        moveSpeed = stats.moveSpeed;
        health = stats.currentHealth;
        dashCooldown = stats.dashCooldown;
        dashSpeed = stats.dashSpeed;
        defense = stats.defense;
        simpleFlash = GetComponentInChildren<SimpleFlash>();

        recoveryCounter = GetComponent<RecoveryCounter>();
        anim = GetComponentInChildren<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
    }


    void FixedUpdate()
    {
        if (!isRolling && !isFrozen && !isKnockedBack)
        {
            // rb.velocity = new Vector2(movement.x * moveSpeed, movement.y * moveSpeed);
            rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
        }
    }

    void Update()
    {
        if (currentCooldown > 0f)
        {
            currentCooldown -= Time.deltaTime;
        }

        HandleInput();

        UpdateAnimator();
    }

    public void LastFacingDirection()
    {

        if (Mathf.Round(Mathf.Abs(horizontal)) == 1 || Mathf.Round(Mathf.Abs(vertical)) == 1)
        {
            //Debug.Log(Mathf.Round(Mathf.Abs(horizontal)));
            anim.SetFloat("LastMoveX", horizontal);
            anim.SetFloat("LastMoveY", vertical);
        }
    }

    // Player.cs
    public void Attack(InputAction.CallbackContext context)
    {
        CombatController.Instance.Attack(context);
    }

    // public void Move(InputAction.CallbackContext context)
    // {
    //     Vector2 input = context.ReadValue<Vector2>();

    //     horizontal = input.x;
    //     vertical = input.y;

    //     movement = input;

    //     LastFacingDirection();
    // }

    int SnapAxis(float value)
    {
        if (value > 0.3f) return 1;
        if (value < -0.3f) return -1;
        return 0;
    }

    Vector2 Snap4Dir(Vector2 input)
    {
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            return new Vector2(Mathf.Sign(input.x), 0);
        else if (Mathf.Abs(input.y) > 0)
            return new Vector2(0, Mathf.Sign(input.y));

        return Vector2.zero;
    }

    public void Move(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        horizontal = SnapAxis(input.x);
        vertical = SnapAxis(input.y);

        movement = new Vector2(horizontal, vertical);

        LastFacingDirection();
    }


    // public void Move(InputAction.CallbackContext context)
    // {
    //     Vector2 input = context.ReadValue<Vector2>();

    //     // Raw movement (can stay analog if you want)
    //     movement = input;
    //     horizontal = input.x;
    //     vertical = input.y;

    //     // Digitalized direction for facing
    //     Vector2 facing = GetCardinalInput(input);

    //     // Animator uses CLEAN values
    //     anim.SetFloat("Horizontal", facing.x);
    //     anim.SetFloat("Vertical", facing.y);
    //     anim.SetFloat("Speed", input.sqrMagnitude);

    //     // Store last move direction cleanly
    //     if (facing != Vector2.zero)
    //     {
    //         lastMoveX = facing.x;
    //         lastMoveY = facing.y;
    //     }
    // }



    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemySpawnRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, itemPickUpRadius);

    }

    public void ShowInventory(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            // Perform actions when the move action starts
            return;

        }
        // Check if the action phase is Cancelled (OnCancelled event)
        else if (context.phase == InputActionPhase.Canceled)
        {
            // Perform actions when the move action is cancelled
            return;

        }
        Debug.Log("opened inventory");

        HUD.Instance.OpenInventory();
    }

    public void ShowForge(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            // Perform actions when the move action starts
            return;

        }
        // Check if the action phase is Cancelled (OnCancelled event)
        else if (context.phase == InputActionPhase.Canceled)
        {
            // Perform actions when the move action is cancelled
            return;

        }
        Debug.Log("opened forge");

        HUD.Instance.OpenForge();
    }


    public void EquipItem(InputAction.CallbackContext context)
    {
        //if the selected itemslot is medallion and there is an item there unequip and put it in the first available itemSlot

        if (context.phase == InputActionPhase.Started)
        {
            return;
        }
        if (context.phase == InputActionPhase.Canceled)
        {
            return;
        }

        if (HUD.Instance.inventoryIsOpen)
        {
            Inventory.Instance.EquipSelectedItem();
            return;
        }
    }

    public void GrabItem(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            return;
        }
        if (context.phase == InputActionPhase.Canceled)
        {
            return;
        }

        if (HUD.Instance.inventoryIsOpen)
        {
            Inventory.Instance.HandleGrab();
            return;
        }

        if (HUD.Instance.forgeIsOpen)
        {
            // ForgeManager.Instance.HandleGrab();
            return;
        }
    }


    void DashTowardsTarget()
    {
        col.isTrigger = true;
        // Move towards the target position
        UnityEngine.Vector2 currentPosition = rb.position;
        UnityEngine.Vector2 direction = (targetPosition - currentPosition).normalized;
        UnityEngine.Vector2 dashPosition = currentPosition + direction * dashSpeed * Time.fixedDeltaTime;

        rb.MovePosition(dashPosition);

        // Check if the player has reached the target position (or is very close to it)
        if (UnityEngine.Vector2.Distance(currentPosition, targetPosition) < 0.1f)
        {
            col.isTrigger = false;
            // isDashing = false; // Stop dashing
        }
    }

    public void HandleInput()
    {

        if (Input.GetKeyDown(KeyCode.P))
        {
            HUD.Instance.PauseGame();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (HUD.Instance.inventoryIsOpen && Input.GetKeyDown(KeyCode.Space))
        {
            var selected = Inventory.Instance.GetSelectedItem();
            if (selected != null && selected.item != null && selected.item.type == ItemType.Recipe)
            {
                Inventory.Instance.LearnSelectedRecipe();
            }
        }
    }

    public void Roll(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        if (currentCooldown <= 0f && !isRolling)
        {
            isRolling = true;
            anim.SetBool("isRolling", true);
            currentCooldown = rollCooldown; // Reset cooldown
            anim.SetTrigger("Roll"); // Play roll animation

            UnityEngine.Vector2 rollDirection = new UnityEngine.Vector2(anim.GetFloat("LastMoveX"), anim.GetFloat("LastMoveY"));
            if (rollDirection == Vector2.zero)
            {
                rollDirection = movement.normalized;
            }
            if (rollDirection == Vector2.zero)
            {
                rollDirection = Vector2.down;
            }
            rollDirection.Normalize();

            horizontal = 0;
            vertical = 0;
            rb.velocity = Vector2.zero;
            rb.AddForce(rollDirection * rollForce, ForceMode2D.Impulse);
            StartCoroutine(StopRolling());
        }
    }

    public IEnumerator StopRolling()
    {
        yield return new WaitForSeconds(0.3f);
        anim.SetBool("isRolling", false);
        //movement = Vector2.zero;
        isRolling = false;
    }


    public void GetHurt(int dmgAmount, Vector2 knockbackDir = default, float knockbackForce = 5f)
    {
        if (!recoveryCounter.recovering)
        {
            recoveryCounter.counter = 0;
            simpleFlash.Flash();
            stats.currentHealth -= dmgAmount;
            SoundManager.Instance?.PlaySFX(hurtSound);
            // anim.SetTrigger("Hurt");
            CameraShake.Instance?.ScreenShake();

            if (knockbackDir != Vector2.zero)
                StartCoroutine(KnockBack(knockbackDir, knockbackForce, 0.15f));

            if (stats.currentHealth <= 0)
            {
                Die();
            }
        }
    }

    private bool isKnockedBack;

    private IEnumerator KnockBack(Vector2 direction, float force, float duration)
    {
        isKnockedBack = true;
        rb.velocity = Vector2.zero;
        float timer = 0f;
        while (timer < duration)
        {
            float t = timer / duration;
            rb.velocity = direction.normalized * Mathf.Lerp(force, 0f, t);
            timer += Time.deltaTime;
            yield return null;
        }
        rb.velocity = Vector2.zero;
        isKnockedBack = false;
    }

        public void HealFromWell()
    {
        var player = Player.Instance;
        if (player == null) return;

        if (player.stats.currentHealth >= player.stats.maxHealth)
        {
            HUD.Instance?.ShowFeedback("Already at full health!");
            return;
        }

        if (player.stats.gold < wellHealCost)
        {
            HUD.Instance?.ShowFeedback("Not enough gold!");
            return;
        }

        player.SpendGold(wellHealCost);
        player.stats.currentHealth = player.stats.maxHealth;
        SoundManager.Instance?.PlaySFX(wellHealSound);
        player.healParticle?.Play();
        HUD.Instance?.ShowFeedback("Healed to full!");
    }

    public void Die()
    {
        HUD.Instance.LostGame();
        //Destroy(this.gameObject, 0.3f);
    }

    void OnCollisionStay2D(Collision2D other)
    {
        // if (other.gameObject.CompareTag("Enemy"))
        // {
        //     GetHurt(other.gameObject.GetComponent<Stats>().damage);
        // }

        //     if (other.gameObject.CompareTag("Enemy"))
        // {
        //     GetHurt(other.gameObject.GetComponent<Stats>().damage);
        // }
    }

    public void GainExp(int xpAmount)
    {
        stats.currentExp += xpAmount;
        if (stats.currentExp >= stats.expNeededToLevelUp)
        {
            LevelUp();
            int increaseAmount = Mathf.RoundToInt(stats.expNeededToLevelUp * 1.2f);
            stats.expNeededToLevelUp = increaseAmount;
        }
    }

    public void ResetStats()
    {
        stats.maxHealth = 100;
        stats.currentHealth = stats.maxHealth;
        stats.currentExp = 0;
        stats.level = 1;
        stats.expNeededToLevelUp = 100;
        stats.moveSpeed = 1.5f;
        stats.damage = 1;
        stats.moveSpeed = 1;
        stats.dashCooldown = 0.5f;
    }

    public void GainHealth(int healthAmount)
    {
        stats.currentHealth += healthAmount;
    }

    public void GainGold(int goldAmount)
    {
        stats.gold += goldAmount;
        HUD.Instance?.PulseGoldCoin();
    }

    public void SpendGold(int goldAmount)
    {
        stats.gold -= goldAmount;
    }

    public void WearEquipment(EquipmentItem itemToWear, bool equip)
    {
        if (equip)
        {
            stats.maxHealth += itemToWear.healthStat;
            stats.damage += itemToWear.damageStat;
            stats.defense += itemToWear.defenseStat;
            stats.moveSpeed += itemToWear.moveSpeedStat;
        }
        else
        {
            stats.maxHealth -= itemToWear.healthStat;
            stats.damage -= itemToWear.damageStat;
            stats.defense -= itemToWear.defenseStat;
            stats.moveSpeed -= itemToWear.moveSpeedStat;
        }
    }
    public void LevelUp()
    {
        // stats.level++;
        // stats.currentExp = 0;
        // HUD.Instance.ShowLevelUpScreen(true);
        // SoundManager.Instance.PlaySFX(levelUpSound);
    }



    public void FreezePlayer(bool freeze)
    {
        isFrozen = freeze;

        if (freeze)
        {
            rb.velocity = Vector2.zero;
            anim.SetFloat("Speed", 0f);
        }
        else
        {
            // Animation updates are OK even while frozen
            // anim.SetFloat("Horizontal", horizontal);
            // anim.SetFloat("Vertical", vertical);
            // anim.SetFloat("Speed", movement.sqrMagnitude);
            LastFacingDirection();
        }
    }

    private void UpdateAnimator()
    {
        if (HUD.Instance.inventoryIsOpen || HUD.Instance.forgeIsOpen || HUD.Instance.shopIsOpen) return;


        if (isFrozen)
        {
            anim.SetFloat("Speed", 0f);
            return;
        }

        anim.SetFloat("Horizontal", movement.x);
        anim.SetFloat("Vertical", movement.y);
        anim.SetFloat("Speed", movement.sqrMagnitude);

        LastFacingDirection();
    }

    public void ResetStatsToBase()
    {
        // BASE STATS (HARDCODED)

        stats.dashSpeed = 1f;
        stats.dashCooldown = 1f;
        stats.moveSpeed = 1.5f;

        stats.maxHealth = 10;
        stats.currentHealth = 10;

        stats.damage = 1;
        stats.defense = 1;

        stats.level = 1;
        stats.currentExp = 0;
        stats.expNeededToLevelUp = 220;

        stats.gold = 0;

        Debug.Log("Player stats reset to base values.");
    }


}

#if UNITY_EDITOR

[CustomEditor(typeof(Player))]
public class PlayerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Player player = (Player)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Reset Player Stats To Base"))
        {
            Undo.RecordObject(player, "Reset Player Stats");

            player.ResetStatsToBase();

            EditorUtility.SetDirty(player);
        }
    }
}

#endif