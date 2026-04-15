using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ForgeManager : MonoBehaviour
{
    [Header("Forge Slots")]
    //items required for recipes
    public ItemSlot oreSlot;
    public ItemSlot fragmentSlot1;
    public ItemSlot fragmentSlot2;

    [Header("Recipes")]
    public List<ForgeRecipe> allRecipes;

    [Header("Material Slots")]
    public List<ItemSlot> materialSlots;
    public Transform inventoryMaterialParent;
    public static ForgeManager Instance;

    [SerializeField] private ItemSlot selectedItemSlot;
    [Header("Crafted Item")]

    public UnityEngine.UI.Image craftedItemImage;
    public GameObject craftedItemObject;
    public Button claimCraftedItemButton;
    private Item craftedItem;
    public SoundData craftSound;

    public SoundData errorSound;
    public SoundData uiSound;




    public void SetSelectedItemSlot(ItemSlot theSelectedSlot)
    {
        selectedItemSlot = theSelectedSlot;
    }

    public ItemSlot GetSelectedItem()
    {
        if (oreSlot.slotIsSelected)
        {
            return oreSlot;
        }
        if (fragmentSlot1.slotIsSelected)
        {
            return fragmentSlot1;
        }
        if (fragmentSlot2.slotIsSelected)
        {
            return fragmentSlot2;
        }

        foreach (ItemSlot selectedSlot in materialSlots)
        {
            if (selectedSlot != null && selectedSlot.slotIsSelected)
            {
                return selectedSlot;
            }
        }
        return null;
    }

    void Awake()
    {
        Instance = this;

        if (Inventory.Instance != null)
            Inventory.Instance.OnInventoryChanged += RefreshMaterialList;

    }

    void OnDestroy()
    {
        if (Inventory.Instance != null)
            Inventory.Instance.OnInventoryChanged -= RefreshMaterialList;
    }


    // Call from Forge Button
    public void TryForge()
    {
        if (!HasValidItems())
        {
            Debug.Log("Missing materials");
        SoundManager.Instance.PlaySFX(errorSound);

            return;
        }

        ForgeRecipe recipe = FindMatchingRecipe();

        if (recipe == null)
        {
            Debug.Log("No matching recipe");
        SoundManager.Instance.PlaySFX(errorSound);

            return;
        }

        Item result = RollResult(recipe);

        craftedItem = result;
        ConsumeMaterials();

        //Temp holder of craftedItem until claimed
        ClearForgeSlots();

        DisplaySuccesfullyCraftedItem();

        Debug.Log("Forged: " + result.name);
    }

    // -----------------------


    public void DisplaySuccesfullyCraftedItem()
    {
        if(craftedItem == null) return;
        craftedItemObject.SetActive(true);
        craftedItemImage.sprite = craftedItem.icon;
        claimCraftedItemButton.Select();
        SoundManager.Instance.PlaySFX(craftSound);

    }

    public void ClaimCraftedItem()
    {
        Inventory.Instance.AddItem(craftedItem, 1);
        craftedItemObject.SetActive(false);
        SelectFirstSlot();
    }






    bool HasValidItems()
    {
        return oreSlot.item != null &&
               fragmentSlot1.item != null &&
               fragmentSlot2.item != null;
    }

    ForgeRecipe FindMatchingRecipe()
    {
        foreach (var recipe in allRecipes)
        {
            if (MatchesRecipe(recipe))
                return recipe;
        }

        return null;
    }

    bool MatchesRecipe(ForgeRecipe recipe)
    {
        if (oreSlot.item != recipe.ore)
            return false;

        Item f1 = fragmentSlot1.item;
        Item f2 = fragmentSlot2.item;

        // Order independent
        return
            (f1 == recipe.fragment1 && f2 == recipe.fragment2) ||
            (f1 == recipe.fragment2 && f2 == recipe.fragment1);
    }

    Item RollResult(ForgeRecipe recipe)
    {
        float roll = Random.Range(0f, 100f);
        float current = 0;

        foreach (var r in recipe.results)
        {
            current += r.chance;

            if (roll <= current)
                return r.resultItem;
        }

        // Safety fallback
        return recipe.results[0].resultItem;
    }

    void ConsumeMaterials()
    {

        Inventory.Instance.RemoveItem(fragmentSlot1.item);
        Inventory.Instance.RemoveItem(fragmentSlot2.item);
        Inventory.Instance.RemoveItem(oreSlot.item);

        oreSlot.RemoveItem(1);
        fragmentSlot1.RemoveItem(1);
        fragmentSlot2.RemoveItem(1);

        RefreshMaterialList();
    }

    public void ClearForgeSlots()
    {
        // if (oreSlot.quantity <= 0)
            oreSlot.ClearSlot();

        // if (fragmentSlot1.quantity <= 0)
            fragmentSlot1.ClearSlot();

        // if (fragmentSlot2.quantity <= 0)
            fragmentSlot2.ClearSlot();
    }

    public void RefreshMaterialList()
    {

        Debug.Log("REFRESHED FORGESLTS");
        // Clear all forge material slots
        foreach (var slot in materialSlots)
        {
            slot.ClearSlot();
        }

        int index = 0;

        foreach (var invSlot in Inventory.Instance.itemSlots)
        {
            if (invSlot.item == null)
                continue;

            if (invSlot.item.type != ItemType.Material)
                continue;

            if (index >= materialSlots.Count)
                break;

            materialSlots[index].AddItem(
                invSlot.item,
                invSlot.quantity
            );

            index++;
        }
    }

public bool TryAutoPlaceMaterial(ItemSlot fromSlot)
{
    if (fromSlot == null || fromSlot.item == null)
        return false;

    if (fromSlot.item.type != ItemType.Material)
        return false;

    MaterialItem mat = fromSlot.item as MaterialItem;
    if (mat == null)
        return false;

    // -------- ORE (single slot → allow swap) --------
    if (mat.materialType == MaterialItem.MaterialType.Ore)
    {
        return TryPlaceOre(fromSlot, oreSlot);
    }

    // -------- FRAGMENTS (only empty slots) --------
    if (mat.materialType == MaterialItem.MaterialType.Fragment)
    {
        if (fragmentSlot1.item == null)
            return TryPlaceFragment(fromSlot, fragmentSlot1);

        if (fragmentSlot2.item == null)
            return TryPlaceFragment(fromSlot, fragmentSlot2);

        // Both full → later holding logic
        // TODO: Implement holding item here

        return false;
    }

    return false;
}

bool TryPlaceOre(ItemSlot from, ItemSlot to)
{
    if (from.item == null)
        return false;

    // Empty → move
    if (to.item == null)
    {
        to.AddItem(from.item, 1);
        from.RemoveItem(1);
        return true;
    }

    // Occupied → swap
    from.SwapItemWithThis(to);
    return true;
}

bool TryPlaceFragment(ItemSlot from, ItemSlot to)
{
    if (from.item == null)
        return false;

    if (to.item != null)
        return false;

    to.AddItem(from.item, 1);
    from.RemoveItem(1);

    return true;
}



    public void HandleGrab()
    {
        ItemSlot selected = GetSelectedItem();

        if (selected == null)
            return;

        // Only allow sending from forge material slots
        if (selected.slotType != SlotType.ForgeSlot)
            return;

        TryAutoPlaceMaterial(selected);
    }


public void SelectFirstSlot()
{
    if (materialSlots.Count == 0)
        return;

    // Clear old selections
    ClearAllSelections();

    // Delay by 1 frame so UI is ready
    StartCoroutine(SelectNextFrame());
    // EventSystem.current.SetSelectedGameObject(materialSlots[0].gameObject);
    // materialSlots[0].ToggleSelection(true);
}

private System.Collections.IEnumerator SelectNextFrame()
{
    yield return null; // wait 1 frame

    EventSystem.current.SetSelectedGameObject(materialSlots[0].gameObject);
    materialSlots[0].ToggleSelection(true);
}
public void ClearAllSelections()
{
    oreSlot.ToggleSelection(false);
    fragmentSlot1.ToggleSelection(false);
    fragmentSlot2.ToggleSelection(false);

    // foreach (var slot in materialSlots)
    // {
    //     slot.ToggleSelection(false);
    // }
}



    bool TryPlaceInSlot(ItemSlot from, ItemSlot to)
{
    if (from.item == null)
        return false;

    // Case 1: target empty → move entire stack
    if (to.item == null)
    {
        to.AddItem(from.item, from.quantity);
        from.ClearSlot();
        return true;
    }

    // Case 2: same item → stack
    if (to.item == from.item && to.item.isStackable)
    {
        to.AddItem(from.item, from.quantity);
        from.ClearSlot();
        return true;
    }

    // Case 3: different item → swap
    from.SwapItemWithThis(to);
    return true;
}


}
