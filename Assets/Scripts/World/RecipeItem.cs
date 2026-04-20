using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Inventory/Recipe")]
public class RecipeItem : Item
{
    public string recipeName;
    public string recipeDescription;
    public ForgeRecipe forgeRecipe;
    // Optionally: public Sprite recipeIcon;
    // Optionally: public List<Item> requiredMaterials;
    // Optionally: public Item resultItem;
}
