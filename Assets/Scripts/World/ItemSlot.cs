using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine.EventSystems;

public enum SlotContains
{
    Any,
    Ore,
    Fragment,
    Equipment,
    AnyMaterial //ore or fragment items
}

public enum SlotType
{
    InventorySlot,
    ForgeSlot,
    RequiredForgeSlot,
    ShopSlot,
    SellSlot
}

public class ItemSlot : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public SlotContains slotContains = SlotContains.Any;
    public SlotType slotType = SlotType.InventorySlot;

    public Item item;
    public int quantity;
    public TextMeshProUGUI itemCountText;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemStatsText;

    public GameObject slotSelectedImageGO;
    public bool slotIsSelected;

    public Image itemSlotImage;
    [Header("Audio")]
    public SoundData uiSound;

    [Header("Selection Pulse")]
    [SerializeField] private float slotPulseScale = 1.12f;
    [SerializeField] private float slotPulseDuration = 0.1f;
    private Coroutine slotPulseCoroutine;
    private Vector3 slotSelectedBaseScale = Vector3.one;

    // Called whenever the item slot needs to be updated

    void Awake()
    {
        if (slotSelectedImageGO != null)
            slotSelectedBaseScale = slotSelectedImageGO.transform.localScale;

        UpdateSlotUI();
    }

    void Start()
    {
        // Inventory.Instance.OnInventoryChanged += UpdateSlotUI;
        UpdateSlotUI();
    }
    public void UpdateSlotUI()
    {
        if (item == null)
        {
            itemSlotImage.color = new Color(
                itemSlotImage.color.r,
                itemSlotImage.color.g,
                itemSlotImage.color.b,
                0f
            );

            itemCountText.text = "";
            return;
        }

        itemSlotImage.color = new Color(
            itemSlotImage.color.r,
            itemSlotImage.color.g,
            itemSlotImage.color.b,
            1f
        );

        itemSlotImage.sprite = item.icon;

        if (quantity > 1)
            itemCountText.text = quantity.ToString();
        else
            itemCountText.text = "";
    }

    public void OnSelect(BaseEventData eventData)
    {
        ToggleSelection(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        ToggleSelection(false);
    }

    public void SetSlotDisplayOnly(Item displayItem)
    {
        item = displayItem;
        quantity = (displayItem != null && displayItem.isStackable) ? 1 : 0;
        UpdateSlotUI();
    }

    // For RequiredForgeSlot: shows owned/required quantity with color feedback
    public void SetRequiredSlot(Item requiredItem, int required = 1)
    {
        slotType = SlotType.RequiredForgeSlot;
        item = requiredItem;

        if (requiredItem == null)
        {
            itemSlotImage.color = new Color(itemSlotImage.color.r, itemSlotImage.color.g, itemSlotImage.color.b, 0f);
            itemCountText.text = "";
            return;
        }

        itemSlotImage.color = Color.white;
        itemSlotImage.sprite = requiredItem.icon;

        int owned = Inventory.Instance != null ? Inventory.Instance.GetItemCount(requiredItem) : 0;
        bool hasEnough = owned >= required;

        itemCountText.text = $"{owned}/{required}";
        itemCountText.color = hasEnough ? Color.white : Color.red;
    }

    public void ToggleSelection(bool isSelected)
    {
        if (slotType == SlotType.ShopSlot)
        {
            HandleShopSlotSelection(isSelected);
            return;
        }

        if (slotType == SlotType.SellSlot)
        {
            HandleSellSlotSelection(isSelected);
            return;
        }

        if (HUD.Instance.inventoryIsOpen)
        {
            HandleInventorySlotSelection(isSelected);
            return;
        }

        if (HUD.Instance.forgeIsOpen)
        {
            HandleForgeSlotSelection(isSelected);
        }
    }

    public void HandleInventorySlotSelection(bool isSelected)
    {
        if (isSelected)
        {
            SoundManager.Instance.PlaySFX(uiSound);
            // Shows the border around the slot
            slotSelectedImageGO.SetActive(true);
            Inventory.Instance.SetSelectedItemSlot(this.gameObject.GetComponent<ItemSlot>());
            slotIsSelected = true;
            PulseSlot();
            if (item != null)
            {
                itemNameText.text = item.name;
                itemStatsText.text = item.GetStatsText();
                //if slot is not empty, change the item actions text depending on itemType
                if (Inventory.Instance.GetSelectedItem().item.type == ItemType.Equipment)
                {
                    if (Inventory.Instance.isHoldingItem == false)
                    {
                        HUD.Instance.ShowFeedback("F - EQUIP/ UNEQUIP    G - grab");
                    }
                    else
                    {
                        HUD.Instance.ShowFeedback("F - EQUIP/ UNEQUIP    G - Place");
                    }
                }
                else if (Inventory.Instance.GetSelectedItem().item.type == ItemType.Recipe)
                {
                    HUD.Instance.ShowFeedback("SPACE - Learn Recipe");
                }
                else
                {
                    if (Inventory.Instance.isHoldingItem == false)
                    {
                        HUD.Instance.ShowFeedback("G - Grab");
                    }
                }
            }
        }
        else
        {
            Inventory.Instance.SetSelectedItemSlot(null);
            itemNameText.text = "empty slot";
            HUD.Instance.ShowFeedback("");
            slotSelectedImageGO.SetActive(false);

            slotIsSelected = false;
        }
    }

    public void HandleForgeSlotSelection(bool isSelected)
    {
        // if (isSelected)
        // {
        //     slotSelectedImageGO.SetActive(true);
        //     ForgeManager.Instance.SetSelectedItemSlot(this.gameObject.GetComponent<ItemSlot>());
        //     slotIsSelected = true;
        //     SoundManager.Instance.PlaySFX(uiSound);
        //     PulseSlot();

        // }
        // else
        // {
        //     ForgeManager.Instance.SetSelectedItemSlot(null);
        //     slotSelectedImageGO.SetActive(false);
        //     slotIsSelected = false;
        // }
    }

    private void PulseSlot()
    {
        if (slotSelectedImageGO == null)
            return;

        if (slotPulseCoroutine != null)
            StopCoroutine(slotPulseCoroutine);

        slotSelectedImageGO.transform.localScale = slotSelectedBaseScale;
        slotPulseCoroutine = StartCoroutine(PulseSlotRoutine());
    }

    private System.Collections.IEnumerator PulseSlotRoutine()
    {
        Transform selectedTransform = slotSelectedImageGO.transform;
        Vector3 originalScale = slotSelectedBaseScale;
        Vector3 targetScale = slotSelectedBaseScale * slotPulseScale;

        float elapsed = 0f;
        while (elapsed < slotPulseDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            selectedTransform.localScale = Vector3.Lerp(originalScale, targetScale, Mathf.Clamp01(elapsed / slotPulseDuration));
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < slotPulseDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            selectedTransform.localScale = Vector3.Lerp(targetScale, originalScale, Mathf.Clamp01(elapsed / slotPulseDuration));
            yield return null;
        }

        selectedTransform.localScale = originalScale;
        slotPulseCoroutine = null;
    }

    public void HandleShopSlotSelection(bool isSelected)
    {
        if (isSelected)
        {
            slotSelectedImageGO.SetActive(true);
            slotIsSelected = true;
            SoundManager.Instance.PlaySFX(uiSound);
            PulseSlot();

            if (ShopManager.Instance != null)
            {
                Debug.Log("Selected shop slot: " + this.name);
                ShopManager.Instance.SetSelectedShopSlot(this);
            }
        }
        else
        {
            // Keep the logical shop item selected even if UI focus moves to the buy button.
            if (ShopManager.Instance != null && ShopManager.Instance.selectedShopSlot == this)
            {
                slotSelectedImageGO.SetActive(true);
                slotIsSelected = true;
                return;
            }

            slotSelectedImageGO.SetActive(false);
            slotIsSelected = false;
        }
    }

    public void HandleSellSlotSelection(bool isSelected)
    {
        if (isSelected)
        {
            slotSelectedImageGO.SetActive(true);
            slotIsSelected = true;

            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.SetSelectedSellSlot(this);
            }
        }
        else
        {
            // Keep the logical sell item selected even if UI focus moves to the sell button.
            if (ShopManager.Instance != null && ShopManager.Instance.selectedSellSlot == this)
            {
                slotSelectedImageGO.SetActive(true);
                slotIsSelected = true;
                return;
            }

            slotSelectedImageGO.SetActive(false);
            slotIsSelected = false;
        }
    }

    public void AddItem(Item newItem, int amount = 1)
    {

        if (newItem == null)
        {
            Debug.Log("new item was null");
            return;
        }

        if (!CanPlaceItem(newItem))
        {
            Debug.Log("Item not allowed in this slot");
            return;
        }

        Debug.Log("Added new item: " + newItem.name);
        if (item == null)
        {
            item = newItem;
            itemSlotImage.sprite = newItem.icon;
            quantity = amount;
        }
        else if (item == newItem && item.isStackable)
        {
            quantity += amount;
        }

        UpdateSlotUI(); // Update the slot after adding an item
    }

    bool CanPlaceItem(Item item)
    {
        if (slotContains == SlotContains.Any)
            return true;

        if (item.type != ItemType.Material)
            return false;

        MaterialItem mat = item as MaterialItem;
        if (mat == null)
            return false;

        if (slotContains == SlotContains.AnyMaterial)
            return true;

        if (slotContains == SlotContains.Ore &&
            mat.materialType == MaterialItem.MaterialType.Ore)
            return true;

        if (slotContains == SlotContains.Fragment &&
            mat.materialType == MaterialItem.MaterialType.Fragment)
            return true;

        return false;
    }

    public void RemoveItem(int amount)
    {
        if (item != null)
        {
            if (item.isStackable)
            {
                quantity -= amount;
                if (quantity <= 0)
                {
                    item = null;
                    quantity = 0;
                }
            }
            else
            {
                // For non-stackable items (like recipes), just clear the slot
                item = null;
                quantity = 0;
            }
        }

        UpdateSlotUI(); // Update the slot after removing an item
    }

    public void SwapItemWithThis(ItemSlot slotToSwapWith)
    {

        // Swap items
        Item tempItem = item;
        item = slotToSwapWith.item;

        slotToSwapWith.item = tempItem;

        if (tempItem != null)
        {
        }

        // Swap quantities if the items are stackable
        if (item.isStackable && slotToSwapWith.item.isStackable)
        {
            int tempQuantity = quantity;
            quantity = slotToSwapWith.quantity;
            slotToSwapWith.quantity = tempQuantity;
        }

        // Update UI for both slots
        UpdateSlotUI();
        slotToSwapWith.UpdateSlotUI();
    }

    public void ClearSlot()
    {
        //item
        item = null;
        quantity = 0;

        UpdateSlotUI(); // Update the slot after clearing it
    }

    public void TrySendToForge()
    {
        // if (item == null)
        //     return;

        // if (ForgeManager.Instance == null)
        //     return;

        // bool success = ForgeManager.Instance.TryAutoPlaceMaterial(this);

        // if (success)
        // {
        //     Debug.Log("Placed material into forge");
        // }
    }
}
