using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class Inventory : MonoBehaviour
{
    [SerializeField] public ItemDatabase itemDatabase; // Reference to the item database
    // [SerializeField] private int capacity = 12; // Maximum capacity of the inventory
    public List<ItemSlot> itemSlots = new List<ItemSlot>(); // List of item slots
    public List<Item> items = new List<Item>(); // List of item slots
    public static Inventory Instance { get; private set; }

    // Event for inventory changes
    public event Action OnInventoryChanged;
    [SerializeField] private ItemSlot grabbedItemSlot;
    [SerializeField] private Item grabbedItem;

    public GameObject grabbedItemGameObject;
    public Image grabbedItemImage;
    public TextMeshProUGUI grabbedItemQuantity;
    public bool isHoldingItem;

    [SerializeField] private ItemSlot weaponSlot;
    [SerializeField] private ItemSlot medallionSlot;

    [SerializeField] private ItemSlot selectedItemSlot;
    public SoundData uiSound;


    public void SetSelectedItemSlot(ItemSlot theSelectedSlot)
    {
        selectedItemSlot = theSelectedSlot;
    }

    public ItemSlot GetSelectedItem()
    {
        if (medallionSlot.slotIsSelected)
        {
            return medallionSlot;
        }
        if (weaponSlot.slotIsSelected)
        {
            return weaponSlot;
        }

        foreach (ItemSlot selectedSlot in itemSlots)
        {
            if (selectedSlot != null && selectedSlot.slotIsSelected)
            {
                return selectedSlot;
            }
        }
        return null;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple Inventory instances found in the scene. Make sure to only have one.");
            Destroy(gameObject);
        }

        if (weaponSlot == null || medallionSlot == null)
        {
            Debug.LogWarning("You have forgotten to ref medallion or weapon slot in the editor come on man.. get yo shit together");
        }

    }

    public void GrabItem(ItemSlot slot)
    {
        isHoldingItem = true;
        grabbedItemGameObject.SetActive(true);
        grabbedItemSlot = slot;
        grabbedItem = slot.item;
        grabbedItemImage.sprite = grabbedItemSlot.item.icon;
        if (grabbedItemSlot.quantity > 1)
        {
            grabbedItemQuantity.text = grabbedItemSlot.quantity.ToString();
        }
        GetSelectedItem().ClearSlot();
    }

    //Being called from Player and calls GrabItem
    public void HandleGrab()
    {
        ItemSlot selected = GetSelectedItem();

        if (selected == null)
            return;

        if (isHoldingItem)
        {
            PlaceItem();
        }
        else
        {
            GrabItem(selected);
        }
    }

    public void PlaceItem()
    {
        selectedItemSlot.AddItem(grabbedItem);
        grabbedItem = null;
        grabbedItemSlot = null;
        isHoldingItem = false;
        grabbedItemGameObject.SetActive(false);

    }

    public void DropItem()
    {
        if (grabbedItem != null && grabbedItem.prefab != null)
        {
            // Calculate the drop position with a slight offset on the y-axis
            Vector2 dropPosition = (Vector2)Player.Instance.transform.position + Vector2.down * 0.4f;

            // Instantiate the prefab at the drop position
            Instantiate(grabbedItem.prefab, dropPosition, Quaternion.identity);

            // Clear the grabbed item and slot
            grabbedItem = null;
            grabbedItemSlot = null;
            isHoldingItem = false;
            grabbedItemGameObject.SetActive(false);
        }
    }

    public void EquipSelectedItem()
    {
        //if the selected itemslot is medallion and there is an item there unequip and put it in the first available itemSlot


        if (GetSelectedItem() == null || GetSelectedItem().item.type != ItemType.Equipment)
        {
            Debug.Log("slot is empty");
            return;
        }
        // Get the selected item slot
        ItemSlot selectedSlot = GetSelectedItem();


        EquipmentItem equipmentItem = (EquipmentItem)selectedSlot.item;

        // Check if the selected item slot is the MedallionSlot and unequip
        if (selectedSlot == medallionSlot)
        {
            UnequipItem(medallionSlot);
            return;
        }

        // Check if the selected item slot is the WeaponSlot and unequip 
        if (selectedSlot == weaponSlot)
        {
            UnequipItem(weaponSlot);
            return;
        }

        // Check the EquipmentType of the selected equipment
        if (equipmentItem.equipmentType == EquipmentType.Medallion)
        {
            // Equip medallion
            if (medallionSlot.item != null)
            {
                medallionSlot.SwapItemWithThis(GetSelectedItem());
            }
            else
            {
                GetSelectedItem().ClearSlot();
                medallionSlot.AddItem(equipmentItem, 1);
                Player.Instance.WearEquipment(equipmentItem, true);
            }
        }
        else
        {
            // Equip weapon
            if (weaponSlot.item != null)
            {
                weaponSlot.SwapItemWithThis(GetSelectedItem());
            }
            else
            {
                GetSelectedItem().ClearSlot();
                weaponSlot.AddItem(equipmentItem, 1);
                Player.Instance.WearEquipment(equipmentItem, true);
            }
        }
        UpdateItemSlots();
    }

    public void UnequipItem(ItemSlot unequipSlot)
    {
        if (unequipSlot == null)
        {
            return;
        }

        ItemSlot firstAvailableSlot = itemSlots[FindFirstAvailableSlotIndex()];
        if (firstAvailableSlot != null)
        {
            Player.Instance.WearEquipment((EquipmentItem)unequipSlot.item, false);
            firstAvailableSlot.SwapItemWithThis(unequipSlot);

            OnInventoryChanged?.Invoke();
        }
        else
        {
            Debug.LogWarning("No available slots to unequip.");
        }
    }


    public void SelectFirstSlot()
    {
        if (itemSlots.Count > 0)
        {
            // Select the first item slot
            EventSystem.current.SetSelectedGameObject(itemSlots[0].gameObject);
            itemSlots[0].ToggleSelection(true);
        }
    }

    // Add an item to the inventory
    public void AddItem(Item item, int quantity = 1)
    {
        if (item == null)
        {
            Debug.LogWarning("Tried to add null item to inventory.");
            return;
        }

        // Find an empty slot or stackable slot for the item
        ItemSlot slot = FindItemSlot(item);

        if (slot != null)
        {
            //slot.quantity += quantity;
            slot.AddItem(item, quantity);
        }
        else
        {
            Debug.LogWarning("Inventory is full.");
            return;
        }

        // Trigger inventory changed event
        OnInventoryChanged?.Invoke();
    }

    public bool CanAddItem(Item item)
    {
        if (item == null)
            return false;

        return FindItemSlot(item) != null;
    }

    // Remove an item from the inventory
    public void RemoveItem(Item item, int quantity = 1)
    {
        if (item == null)
        {
            Debug.LogWarning("Tried to remove null item from inventory.");
            return;
        }

        // Find the slot containing the item
        ItemSlot slot = FindItemSlot(item);

        if (slot != null)
        {
            slot.quantity -= quantity;

            if (slot.quantity <= 0)
            {
                slot.item = null;
            }

            // Trigger inventory changed event
            OnInventoryChanged?.Invoke();
        }
    }

    private int FindFirstAvailableSlotIndex()
    {


        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (itemSlots[i].item == null)
            {
                return i;
            }
        }

        return -1;
        // foreach (var slot in itemSlots)
        // {
        //     if (slot.item == null)
        //     {
        //         // If the slot is empty, return it
        //         return slot;
        //     }
        // }
        // return null;
    }

    // Find an empty slot or stackable slot for the item
    private ItemSlot FindItemSlot(Item item)
    {
        foreach (var slot in itemSlots)
        {
            if (slot.item == null)
            {
                // If the slot is empty, return it
                return slot;
            }
            else if (slot.item == item && slot.quantity < item.maxStack)
            {
                // If the slot has the same item and is not at max stack, return it
                return slot;
            }
        }
        return null;
    }

    // Update item slots based on inventory contents
    private void UpdateItemSlots()
    {
        for (int i = 0; i < itemSlots.Count; i++)
        {
            // Update item slot UI element with corresponding item data from inventory
            itemSlots[i].UpdateSlotUI();
        }

        medallionSlot.UpdateSlotUI();
        weaponSlot.UpdateSlotUI();
    }
}
