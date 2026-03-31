using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

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

public class ItemSlot : MonoBehaviour
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

    // Called whenever the item slot needs to be updated

    void Awake()
    {

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
            // SoundManager.Instance.PlaySFX(uiSound);
            // Shows the border around the slot
            slotSelectedImageGO.SetActive(true);
            Inventory.Instance.SetSelectedItemSlot(this.gameObject.GetComponent<ItemSlot>());
            slotIsSelected = true;
            if (item != null)
            {
                itemNameText.text = item.name;
                itemStatsText.text = item.GetStatsText();
                //if slot is not empty, change the item actions text depending on itemType
                if (Inventory.Instance.GetSelectedItem().item.type == ItemType.Equipment)
                {
                    if (Inventory.Instance.isHoldingItem == false)
                    {
                        HUD.Instance.infoText.text = "F - EQUIP/ UNEQUIP    G - grab";
                    }
                    else
                    {
                        HUD.Instance.infoText.text = "F - EQUIP/ UNEQUIP    G - Place";
                    }
                }
                else
                {
                    if (Inventory.Instance.isHoldingItem == false)
                    {
                        HUD.Instance.infoText.text = "G - Grab";
                    }
                }
            }
        }
        else
        {
            Inventory.Instance.SetSelectedItemSlot(null);
            itemNameText.text = "empty slot";
            HUD.Instance.infoText.text = "";
            slotSelectedImageGO.SetActive(false);

            slotIsSelected = false;
        }
    }

    public void HandleForgeSlotSelection(bool isSelected)
    {
        if (isSelected)
        {
            slotSelectedImageGO.SetActive(true);
            ForgeManager.Instance.SetSelectedItemSlot(this.gameObject.GetComponent<ItemSlot>());
            slotIsSelected = true;
            // SoundManager.Instance.PlaySFX(uiSound);

        }
        else
        {
            ForgeManager.Instance.SetSelectedItemSlot(null);
            slotSelectedImageGO.SetActive(false);
            slotIsSelected = false;
        }
    }

    public void HandleShopSlotSelection(bool isSelected)
    {
        if (isSelected)
        {
            slotSelectedImageGO.SetActive(true);
            slotIsSelected = true;
            ShopManager.Instance?.SetSelectedShopSlot(this);
        }
        else
        {
            if (ShopManager.Instance?.selectedShopSlot == this)
            {
                ShopManager.Instance.SetSelectedShopSlot(null);
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
            ShopManager.Instance?.SetSelectedSellSlot(this);
        }
        else
        {
            if (ShopManager.Instance?.selectedSellSlot == this)
            {
                ShopManager.Instance.SetSelectedSellSlot(null);
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
        if (item != null && item.isStackable)
        {
            quantity -= amount;
            if (quantity <= 0)
            {
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
        if (item == null)
            return;

        if (ForgeManager.Instance == null)
            return;

        bool success = ForgeManager.Instance.TryAutoPlaceMaterial(this);

        if (success)
        {
            Debug.Log("Placed material into forge");
        }
    }
}
