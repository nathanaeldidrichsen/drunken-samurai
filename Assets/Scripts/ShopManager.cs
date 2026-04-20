using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class ShopProduct
{
    public Item item;
    public int buyPrice;
    public int quantity = 1;
}

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("Shop Settings")]
    public List<ShopProduct> shopProducts = new List<ShopProduct>();
    public int buyMultiplier = 2;
    public int sellMultiplier = 1;

    [Header("UI References")]
    public GameObject shopScreen;
    public List<ItemSlot> shopItemSlots = new List<ItemSlot>();
    public List<ItemSlot> sellItemSlots = new List<ItemSlot>();
    public TextMeshProUGUI shopCurrencyText;
    public TextMeshProUGUI selectedShopNameText;
    public TextMeshProUGUI selectedShopDescriptionText;
    public TextMeshProUGUI selectedShopPriceText;
    public TextMeshProUGUI selectedSellNameText;
    public TextMeshProUGUI selectedSellDescriptionText;
    public TextMeshProUGUI selectedSellPriceText;
    public TextMeshProUGUI feedbackText;
    public Button buyButton;
    public ToggleSelectedButton buySelectedToggle;
    public Button sellButton;
    public ToggleSelectedButton sellSelectedToggle;

    [Header("Audio")]
    public SoundData buyCoinSound;
    public SoundData uiSound;


    [Header("Confirm UX")]
    [SerializeField] private float buttonPulseScale = 1.08f;
    [SerializeField] private float buttonPulseDuration = 0.12f;

    private Coroutine pulseCoroutine;
    private enum ShopAction { None, Buy, Sell }

    [HideInInspector] public ItemSlot selectedShopSlot;
    [HideInInspector] public ItemSlot selectedSellSlot;
    private ShopAction currentAction = ShopAction.None;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // if (Inventory.Instance != null)
        //     Inventory.Instance.OnInventoryChanged += RefreshSellSlots;

        RefreshShopSlots();
        // RefreshSellSlots();
        UpdateCurrencyDisplay();
        UpdateSelectedShopInfo();
        // UpdateSelectedSellInfo();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            OpenShop();
            return;
        }

        if (shopScreen != null && shopScreen.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseShop();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                HandleConfirmInput();
            }
        }
    }

    private void OnDestroy()
    {
        // if (Inventory.Instance != null)
        //     Inventory.Instance.OnInventoryChanged -= RefreshSellSlots;
    }

    public void OpenShop()
    {
        if (shopScreen == null) return;
        if (HUD.Instance.inventoryIsOpen || HUD.Instance.forgeIsOpen) return;

        bool shouldOpen = !shopScreen.activeSelf;
        shopScreen.SetActive(shouldOpen);
        HUD.Instance.shopIsOpen = shouldOpen;

        if (shouldOpen)
        {
            if (Player.Instance != null)
                Player.Instance.FreezePlayer(true);

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);

            RefreshShopSlots();
            // RefreshSellSlots();
            SelectFirstShopSlot();
            currentAction = ShopAction.None;
            SetButtonSelectedState(false, false);
            UpdateCurrencyDisplay();
            feedbackText.text = string.Empty;
            Time.timeScale = 0f;
        }
        else
        {
            CloseShop();
        }
    }

    public void CloseShop()
    {
        if (shopScreen == null) return;
        shopScreen.SetActive(false);
        HUD.Instance.shopIsOpen = false;
        if (Player.Instance != null)
            Player.Instance.FreezePlayer(false);

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        Time.timeScale = 1f;
        currentAction = ShopAction.None;
        SetButtonSelectedState(false, false);
        ClearShopSelection();
        ClearSellSelection();
    }

    public void SetSelectedShopSlot(ItemSlot slot)
    {
        bool slotChanged = selectedShopSlot != slot;
        selectedShopSlot = slot;
        if (slotChanged)
        {
            currentAction = ShopAction.None;
            SetButtonSelectedState(false, false);
        }
        DeselectAll(shopItemSlots, slot);
        UpdateSelectedShopInfo();
    }

    public void SetSelectedSellSlot(ItemSlot slot)
    {
        selectedSellSlot = slot;
        currentAction = ShopAction.None;
        SetButtonSelectedState(false, false);
        // selectedSellSlot = slot;
        // currentAction = ShopAction.None;
        // selectedShopSlot = null;
        // DeselectAll(sellItemSlots, slot);
        // DeselectAll(shopItemSlots, null);

        // if (slot != null)
        // {
        //     slot.slotIsSelected = true;
        //     if (slot.slotSelectedImageGO != null)
        //         slot.slotSelectedImageGO.SetActive(true);
        // }

        // UpdateSelectedSellInfo();
        // UpdateSelectedShopInfo();
    }

    private void HandleConfirmInput()
    {
        if (selectedShopSlot != null && selectedShopSlot.item != null)
        {
            if (currentAction != ShopAction.Buy)
            {
                currentAction = ShopAction.Buy;
                ClearSellSelection();
                UpdateSelectedShopInfo();
                SetButtonSelectedState(true, false);
                if (buyButton != null)
                {
                    // buyButton.Select();
                    buySelectedToggle.ToggleSelected(true);
                    SoundManager.Instance?.PlaySFX(uiSound);

                    // PulseButton(buyButton);
                }

                ShowFeedback($"Press Space to purchase.");
            }
            else
            {
                BuySelectedItem();
            }
            return;
        }

        // Sell functionality disabled for now
        // if (selectedSellSlot != null && selectedSellSlot.item != null)
        // {
        //     if (currentAction != ShopAction.Sell)
        //     {
        //         currentAction = ShopAction.Sell;
        //         ClearShopSelection();
        //         SetButtonSelectedState(false, true);
        //         if (sellButton != null)
        //         {
        //             sellButton.Select();
        //             sellSelectedToggle.ToggleSelected(true);
        //             PulseButton(sellButton);
        //         }
        //
        //         ShowFeedback($"Press Space again to sell {selectedSellSlot.item.itemName}.");
        //     }
        //     else
        //     {
        //         SellSelectedItem();
        //     }
        // }
    }

    public void BuySelectedItem()
    {
        if (selectedShopSlot == null || selectedShopSlot.item == null)
            return;

        Item itemToBuy = selectedShopSlot.item;
        int buyPrice = GetBuyPrice(itemToBuy);

        if (Player.Instance.stats.gold < buyPrice)
        {
            ShowFeedback("Not enough gold.");
            return;
        }

        if (Inventory.Instance == null)
        {
            ShowFeedback("Inventory not available.");
            return;
        }

        if (!Inventory.Instance.CanAddItem(itemToBuy))
        {
            ShowFeedback("Inventory is full.");
            return;
        }

        Player.Instance.SpendGold(buyPrice);
        Inventory.Instance.AddItem(itemToBuy, 1);
        SoundManager.Instance?.PlaySFX(buyCoinSound);
        HUD.Instance?.PulseGoldCoin();
        // RefreshSellSlots();
        currentAction = ShopAction.None;
        SetButtonSelectedState(false, false);
        UpdateCurrencyDisplay();
        ShowFeedback($"Bought {itemToBuy.itemName}.");
    }

    public void SellSelectedItem()
    {
        // Sell functionality disabled for now
        // if (selectedSellSlot == null || selectedSellSlot.item == null)
        //     return;
        //
        // Item itemToSell = selectedSellSlot.item;
        // int sellPrice = GetSellPrice(itemToSell);
        //
        // Inventory.Instance.RemoveItem(itemToSell, 1);
        // Player.Instance.stats.gold += sellPrice;
        // RefreshSellSlots();
        // currentAction = ShopAction.None;
        // SetButtonSelectedState(false, false);
        // UpdateCurrencyDisplay();
        // ShowFeedback($"Sold {itemToSell.itemName}.");
    }

    public void RefreshShopSlots()
    {
        for (int i = 0; i < shopItemSlots.Count; i++)
        {
            ItemSlot slot = shopItemSlots[i];
            slot.ClearSlot();
            slot.slotType = SlotType.ShopSlot;

            if (i < shopProducts.Count && shopProducts[i].item != null)
            {
                var product = shopProducts[i];
                slot.AddItem(product.item, product.quantity);
            }
        }

        ClearShopSelection();
        UpdateSelectedShopInfo();
    }

    public void RefreshSellSlots()
    {
        // Sell functionality disabled for now
        // if (Inventory.Instance == null) return;
        //
        // int index = 0;
        //
        // foreach (var invSlot in Inventory.Instance.itemSlots)
        // {
        //     if (invSlot.item == null)
        //         continue;
        //
        //     if (index >= sellItemSlots.Count)
        //         break;
        //
        //     ItemSlot slot = sellItemSlots[index];
        //     slot.ClearSlot();
        //     slot.slotType = SlotType.SellSlot;
        //     slot.AddItem(invSlot.item, invSlot.quantity);
        //     index++;
        // }
        //
        // for (int i = index; i < sellItemSlots.Count; i++)
        // {
        //     sellItemSlots[i].ClearSlot();
        //     sellItemSlots[i].slotType = SlotType.SellSlot;
        // }
        //
        // ClearSellSelection();
        // UpdateSelectedSellInfo();
    }

    public void UpdateCurrencyDisplay()
    {
        if (shopCurrencyText == null) return;
        shopCurrencyText.text = Player.Instance.stats.gold.ToString();
    }

    private void UpdateSelectedShopInfo()
    {
        if (selectedShopSlot != null && selectedShopSlot.item != null)
        {
            var item = selectedShopSlot.item;
            // Show recipe info if RecipeItem
            if (item is RecipeItem recipe)
            {
                selectedShopNameText.text = !string.IsNullOrEmpty(recipe.recipeName) ? recipe.recipeName : item.itemName;
                selectedShopDescriptionText.text = !string.IsNullOrEmpty(recipe.recipeDescription) ? recipe.recipeDescription : item.itemDescription;
            }
            else
            {
                selectedShopNameText.text = item.itemName;
                selectedShopDescriptionText.text = item.itemDescription;
            }
            selectedShopPriceText.text = $"Costs: {GetBuyPrice(item)} coins";
            buyButton.interactable = Player.Instance.stats.gold >= GetBuyPrice(item);
        }
        else
        {
            selectedShopNameText.text = "No item selected";
            selectedShopDescriptionText.text = string.Empty;
            selectedShopPriceText.text = "Buy: -";
            buyButton.interactable = false;
        }
    }

    private void UpdateSelectedSellInfo()
    {
        // Sell functionality disabled for now
        // if (selectedSellSlot != null && selectedSellSlot.item != null)
        // {
        //     var item = selectedSellSlot.item;
        //     selectedSellNameText.text = item.itemName;
        //     selectedSellDescriptionText.text = item.itemDescription;
        //     selectedSellPriceText.text = $"Sell: {GetSellPrice(item)}";
        //     sellButton.interactable = true;
        // }
        // else
        // {
        //     selectedSellNameText.text = "No item selected";
        //     selectedSellDescriptionText.text = string.Empty;
        //     selectedSellPriceText.text = "Sell: -";
        //     sellButton.interactable = false;
        // }
    }


    private int GetBuyPrice(Item item)
    {
        if (item == null) return 0;

        var product = shopProducts.Find(p => p.item == item);
        if (product != null && product.buyPrice > 0)
            return product.buyPrice;

        return Mathf.Max(item.sellPrice * buyMultiplier, 1);
    }

    private int GetSellPrice(Item item)
    {
        if (item == null) return 0;
        return Mathf.Max(item.sellPrice * sellMultiplier, 0);
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText == null) return;
        feedbackText.text = message;
    }

    private void SetButtonSelectedState(bool buySelected, bool sellSelected)
    {
        if (buySelectedToggle != null)
        {
            buySelectedToggle.ToggleSelected(buySelected);
        }

        if (sellSelectedToggle != null)
        {
            sellSelectedToggle.ToggleSelected(sellSelected);
        }
    }

    private void PulseButton(Button button)
    {
        if (button == null)
            return;

        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }

        pulseCoroutine = StartCoroutine(PulseButtonRoutine(button));
    }

    private IEnumerator PulseButtonRoutine(Button button)
    {
        if (button == null)
            yield break;

        Transform target = button.transform;
        Vector3 originalScale = target.localScale;
        Vector3 targetScale = originalScale * buttonPulseScale;

        float elapsed = 0f;
        while (elapsed < buttonPulseDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / buttonPulseDuration);
            target.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < buttonPulseDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / buttonPulseDuration);
            target.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        target.localScale = originalScale;
        pulseCoroutine = null;
    }

    private void ClearShopSelection()
    {
        selectedShopSlot = null;
        DeselectAll(shopItemSlots, null);
        UpdateSelectedShopInfo();
    }

    private void ClearSellSelection()
    {
        selectedSellSlot = null;
        DeselectAll(sellItemSlots, null);
        // UpdateSelectedSellInfo();
    }

    private void DeselectAll(List<ItemSlot> slots, ItemSlot except)
    {
        if (slots == null) return;

        foreach (var slot in slots)
        {
            if (slot == null || slot == except) continue;
            if (slot.slotIsSelected)
            {
                slot.slotIsSelected = false;
                if (slot.slotSelectedImageGO != null)
                    slot.slotSelectedImageGO.SetActive(false);
            }
        }
    }

    public void SelectFirstShopSlot()
    {
        if (shopItemSlots == null || shopItemSlots.Count == 0)
            return;

        ClearShopSelection();
        StartCoroutine(SelectFirstShopSlotNextFrame());
    }

    private IEnumerator SelectFirstShopSlotNextFrame()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();

        if (shopItemSlots == null || shopItemSlots.Count == 0)
            yield break;

        ItemSlot firstSlot = null;

        foreach (var slot in shopItemSlots)
        {
            if (slot != null && slot.item != null)
            {
                firstSlot = slot;
                break;
            }
        }

        if (firstSlot == null)
        {
            firstSlot = shopItemSlots[0];
        }

        if (firstSlot == null)
            yield break;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSlot.gameObject);
        }
        else
        {
            firstSlot.ToggleSelection(true);
        }
    }
}
