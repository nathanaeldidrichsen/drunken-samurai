using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Forge Recipe", menuName = "Crafting/Forge Recipe")]
public class ForgeRecipe : ScriptableObject
{
    [Header("Ingredients")]
    public Item ore;
    public Item fragment1;
    public Item fragment2;

    [Header("Results")]
    public List<ForgeResult> results;
}

[System.Serializable]
public class ForgeResult
{
    public Item resultItem;
    [Range(0,100)]
    public float chance;
}
