using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ForgeManager : MonoBehaviour
{
    [Header("List UI")]
    public Transform recipeListParent;

    public Transform materialListParent;
    public GameObject listItemPrefab;

    private List<ListItem> recipeListItems = new();
    private List<ListItem> materialListItems = new();

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

    [SerializeField] private ListItem selectedListItem;
    [Header("Crafted Item")]

    public UnityEngine.UI.Image craftedItemImage;
    public GameObject craftedItemObject;
    public Button claimCraftedItemButton;
    public Button forgeButton;

    private Item craftedItem;
    public SoundData craftSound;

    public SoundData errorSound;
    public SoundData uiSound;



    public void SetSelectedListItem(ListItem newSelectedListItem)
    {
        selectedListItem = newSelectedListItem;
    }



    void Awake()    {
        Instance = this;

        RefreshRecipeList();
        RefreshMaterialList();

        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnInventoryChanged += RefreshMaterialList;
            Inventory.Instance.OnInventoryChanged += RefreshRecipeList;
            Inventory.Instance.OnInventoryChanged += RefreshSelectedRecipeSlots;
        }

    }

    void OnDestroy()
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnInventoryChanged -= RefreshMaterialList;
            Inventory.Instance.OnInventoryChanged -= RefreshRecipeList;
            Inventory.Instance.OnInventoryChanged -= RefreshSelectedRecipeSlots;
        }
    }

    public void SelectFirstRecipeListItem()
    {
        if (recipeListItems.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(recipeListItems[0].gameObject);
            recipeListItems[0].SetSelected(true);
        }
    }


    // Call from Forge Button
    public void TryForge()
    {
        // Check that a recipe is selected and the player has all required materials in inventory
        if (selectedListItem == null || selectedListItem.item is not RecipeItem selectedRecipe || selectedRecipe.forgeRecipe == null)
        {
            Debug.Log("No recipe selected");
            SoundManager.Instance.PlaySFX(errorSound);
            return;
        }

        ForgeRecipe recipe = selectedRecipe.forgeRecipe;

        if (!Inventory.Instance.HasItem(recipe.ore) ||
            !Inventory.Instance.HasItem(recipe.fragment1) ||
            !Inventory.Instance.HasItem(recipe.fragment2))
        {
            Debug.Log("Missing materials in inventory");
            SoundManager.Instance.PlaySFX(errorSound);
            return;
        }

        Item result = RollResult(recipe);

        craftedItem = result;
        ConsumeMaterials(recipe);

        //Temp holder of craftedItem until claimed
        // ClearForgeSlots();

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
        SelectFirstListItem();
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

    void ConsumeMaterials(ForgeRecipe recipe)
    {
        Inventory.Instance.RemoveItem(recipe.ore);
        Inventory.Instance.RemoveItem(recipe.fragment1);
        Inventory.Instance.RemoveItem(recipe.fragment2);

        RefreshMaterialList();
        SetForgeSlotsForRecipe(recipe);
    }

    public void RefreshMaterialList()
    {

        // Clear old material list items
        foreach (Transform child in materialListParent)
            Destroy(child.gameObject);
        materialListItems.Clear();

        // Add all materials/ores from inventory
        foreach (var invSlot in Inventory.Instance.itemSlots)
        {
            if (invSlot.item == null) continue;
            if (invSlot.item.type != ItemType.Material) continue;
            var go = Instantiate(listItemPrefab, materialListParent);
            var li = go.GetComponent<ListItem>();
            li.Setup(invSlot.item, invSlot.quantity);
            materialListItems.Add(li);
        }
    }

    public void RefreshRecipeList()
    {
        // Clear old recipe list items
        foreach (Transform child in recipeListParent)
            Destroy(child.gameObject);
        recipeListItems.Clear();

        // Use learnedRecipes directly — each RecipeItem already has a .forgeRecipe reference
        foreach (var recipeItem in Inventory.Instance.learnedRecipes)
        {
            if (recipeItem == null || recipeItem.forgeRecipe == null) continue;
            var go = Instantiate(listItemPrefab, recipeListParent);
            var li = go.GetComponent<ListItem>();
            li.Setup(recipeItem, 1);
            recipeListItems.Add(li);
        }

        // Wire explicit navigation: up/down through list, left/right goes to forge button
        for (int i = 0; i < recipeListItems.Count; i++)
        {
            var button = recipeListItems[i].GetComponent<Button>();
            if (button == null) continue;

            var nav = new Navigation { mode = Navigation.Mode.Explicit };
            nav.selectOnUp    = i > 0 ? recipeListItems[i - 1].GetComponent<Button>() : null;
            nav.selectOnDown  = i < recipeListItems.Count - 1 ? recipeListItems[i + 1].GetComponent<Button>() : null;
            nav.selectOnRight = forgeButton;
            nav.selectOnLeft  = forgeButton;
            button.navigation = nav;
        }

        // Wire forge button navigation: left/right goes to first recipe list item, up/down stays on forge button
        var forgeNav = new Navigation { mode = Navigation.Mode.Explicit };
        var firstRecipeButton = recipeListItems.Count > 0 ? recipeListItems[0].GetComponent<Button>() : null;
        forgeNav.selectOnLeft  = firstRecipeButton;
        forgeNav.selectOnRight = firstRecipeButton;
        forgeNav.selectOnUp    = forgeButton;
        forgeNav.selectOnDown  = forgeButton;
        forgeButton.navigation = forgeNav;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var selected = EventSystem.current.currentSelectedGameObject;
            if (selected == claimCraftedItemButton.gameObject)
            {
                ClaimCraftedItem();
            }
            else if (selected == forgeButton.gameObject)
            {
                TryForge();
            }
            else if (selectedListItem != null && selectedListItem.item is RecipeItem)
            {
                forgeButton.Select();
            }
        }
    }

    public void OnListItemSelected(ListItem item)
    {
        // If a RecipeItem is selected, show required materials in forge slots using its forgeRecipe
        if (item.item is RecipeItem recipe && recipe.forgeRecipe != null)
        {
            ShowRecipeRequirements(recipe.forgeRecipe);
            SetForgeSlotsForRecipe(recipe.forgeRecipe);
            UpdateForgeButtonState(recipe.forgeRecipe);
        }
        // Optionally, handle material selection here
    }

    private void RefreshSelectedRecipeSlots()
    {
        if (selectedListItem != null && selectedListItem.item is RecipeItem recipe && recipe.forgeRecipe != null)
        {
            SetForgeSlotsForRecipe(recipe.forgeRecipe);
            UpdateForgeButtonState(recipe.forgeRecipe);
        }
    }

    private void UpdateForgeButtonState(ForgeRecipe recipe)
    {
        bool hasAll = Inventory.Instance.HasItem(recipe.ore) &&
                      Inventory.Instance.HasItem(recipe.fragment1) &&
                      Inventory.Instance.HasItem(recipe.fragment2);
        forgeButton.interactable = hasAll;
    }

    private void SetForgeSlotsForRecipe(ForgeRecipe forgeRecipe)
    {
        // Display required material in each slot with owned/required quantity and color feedback
        oreSlot.SetRequiredSlot(forgeRecipe.ore, 1);
        fragmentSlot1.SetRequiredSlot(forgeRecipe.fragment1, 1);
        fragmentSlot2.SetRequiredSlot(forgeRecipe.fragment2, 1);
    }

    private void ShowRecipeRequirements(ForgeRecipe forgeRecipe)
    {
        // Example: highlight or display required ore/fragment icons and amounts
        Debug.Log($"Recipe requires: {forgeRecipe.ore?.itemName}, {forgeRecipe.fragment1?.itemName}, {forgeRecipe.fragment2?.itemName}");
        // UI update is handled in SetForgeSlotsForRecipe
    }

    public void SelectFirstListItem()
    {
        // Improved navigation: select the first recipe list item if available
        if (recipeListItems.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(recipeListItems[0].gameObject);
            recipeListItems[0].SetSelected(true);
            OnListItemSelected(recipeListItems[0]);
        }
        else if (materialListItems.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(materialListItems[0].gameObject);
            materialListItems[0].SetSelected(true);
        }
    }
}