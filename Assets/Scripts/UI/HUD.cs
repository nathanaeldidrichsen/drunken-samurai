using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Unity.VisualScripting;


/*Manages and updates the HUD, which contains your health bar, coins, etc*/

public class HUD : MonoBehaviour
{
    [Header("Reference")]
    // public Animator anim;
    //[SerializeField] private GameObject healthBar;
    //[SerializeField] private GameObject startUp;
    [SerializeField] private GameObject deadScreen;
    [SerializeField] private GameObject pauseScreen;
    private bool isPaused = false;

    [SerializeField] private GameObject levelUpScreen;
    [SerializeField] private GameObject inventoryScreen;
    [SerializeField] private GameObject forgeScreen;

    [Header("Audio")]
    public SoundData openMenuSound;

    public SoundData gemSound;

    public TextMeshProUGUI lvlText;
    public TextMeshProUGUI honorText;
    public TextMeshProUGUI infoText;

    public TextMeshProUGUI uiHealth;
    public TextMeshProUGUI uiDmg;
    public TextMeshProUGUI uiDef;
    public TextMeshProUGUI uiSpeed;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider expSlider;
    private Animator anim;

    [System.NonSerialized] public string loadSceneName;
    [System.NonSerialized] public bool resetPlayer;
    public bool inventoryIsOpen;
    public bool forgeIsOpen;


    private static HUD instance;
    public static HUD Instance
    {
        get
        {
            if (instance == null) instance = GameObject.FindObjectOfType<HUD>();
            return instance;
        }
    }

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        Time.timeScale = 1;
        deadScreen.SetActive(false);
        //Set all bar widths to 1, and also the smooth variables.
        //healthBarWidth = 1;
        //healthBarWidthEased = healthBarWidth;
    }

    void Update()
    {
        if (healthSlider != null)
        {
            healthSlider.value = Player.Instance.stats.currentHealth;
            healthSlider.maxValue = Player.Instance.stats.maxHealth;
        }

        expSlider.value = Player.Instance.stats.currentExp;
        expSlider.maxValue = Player.Instance.stats.expNeededToLevelUp;
        lvlText.text = Player.Instance.stats.level.ToString();
        honorText.text = Player.Instance.stats.gold.ToString();
        uiHealth.text = Player.Instance.stats.maxHealth.ToString();
        uiDmg.text = Player.Instance.stats.damage.ToString();
        uiDef.text = Player.Instance.stats.defense.ToString();
        uiSpeed.text = Player.Instance.stats.moveSpeed.ToString();

        //Controls the width of the health bar based on the player's total health
        //healthBarWidth = (float)Player.Instance.health / (float)Player.Instance.maxHealth;
        // healthBarWidthEased += (healthBarWidth - healthBarWidthEased) * Time.deltaTime * healthBarWidthEased;
        // healthBar.transform.localScale = new Vector2(healthBarWidthEased, 1);
    }

    public void PlayCoverScreenAnimation()
    {
        anim.CrossFadeInFixedTime("hud_startscene", 0.01f);
    }

    public void OpenInventory()
    {
        if (Instance.forgeIsOpen) return;


        SoundManager.Instance.PlaySFX(openMenuSound);
        if (inventoryScreen.activeSelf)
        {
            inventoryIsOpen = false;
            Time.timeScale = 1;
            if (Inventory.Instance.isHoldingItem)
            {
                Inventory.Instance.DropItem();
            }
            inventoryScreen.SetActive(false);
        }
        else
        {
            inventoryIsOpen = true;
            inventoryScreen.SetActive(true);
            Inventory.Instance.SelectFirstSlot();
            Time.timeScale = 0;
        }
    }

    public void OpenForge()
    {

        if (Instance.inventoryIsOpen) return;
        SoundManager.Instance.PlaySFX(openMenuSound);
        // SoundManager.Instance.PlaySFX(openMenuSound);
        if (forgeScreen.activeSelf)
        {
            Time.timeScale = 1;
            ForgeManager.Instance.ClearAllSelections();
            ForgeManager.Instance.ClearForgeSlots();
            forgeScreen.SetActive(false);
            forgeIsOpen = false;
        }
        else
        {
            forgeScreen.SetActive(true);
            ForgeManager.Instance.RefreshMaterialList();
            ForgeManager.Instance.SelectFirstSlot();
            forgeIsOpen = true;
            Time.timeScale = 0;

        }
    }

    public void HealthBarHurt()
    {
        //animator.SetTrigger("hurt");
    }

    public void LostGame()
    {
        deadScreen.SetActive(true);
        Time.timeScale = 0;
    }

    public void IncreaseDamage()
    {
        // Player.Instance.stats.damage = (int)(Player.Instance.stats.damage * 1.1f); // Increase damage by 10%
        // int increaseAmount = Mathf.RoundToInt(Player.Instance.stats.damage * 0.1f);
        // Player.Instance.stats.damage += increaseAmount;

        Player.Instance.stats.damage++;
    }

    public void IncreaseMoveSpeed()
    {
        Player.Instance.stats.moveSpeed *= 1.2f; // Increase moveSpeed by 10%
    }

    public void IncreaseHealth()
    {
        int increaseAmount = Mathf.RoundToInt(Player.Instance.stats.maxHealth * 0.1f);
        Player.Instance.stats.maxHealth += increaseAmount;
        Player.Instance.stats.currentHealth = Player.Instance.stats.maxHealth;
    }

    public void DecreaseDashCooldown()
    {
        Player.Instance.stats.dashCooldown *= 0.8f; // Decrease dashCooldown by 10%
    }




    public void ShowLevelUpScreen(bool showLevelUpScreen)
    {
        levelUpScreen.SetActive(showLevelUpScreen);

        if (showLevelUpScreen)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    public void PauseGame()
    {
        SoundManager.Instance.PlaySFX(gemSound);

        if (!isPaused)
        {
            pauseScreen.SetActive(true);
            isPaused = true;
            Time.timeScale = 0;
        }
        else
        {
            pauseScreen.SetActive(false);
            isPaused = false;
            Time.timeScale = 1;
        }
    }
}
