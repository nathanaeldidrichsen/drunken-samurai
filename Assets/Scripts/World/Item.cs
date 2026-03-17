using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public int id;

    public Sprite icon;
    public ItemType type;
    public int maxStack;
    public bool isStackable;
    public bool isEquipable;
    public bool isCraftable;
    public int sellPrice;
    public string itemDescription;
    public GameObject prefab;
    public bool isEquipped; // Flag to indicate if the item is equipped

    // Add any other properties or methods relevant to your game


    public string GetStatsText()
    {
        if (type == ItemType.Equipment)
        {
            EquipmentItem equipmentItem = this as EquipmentItem;
            if (equipmentItem != null)
            {
                return $"Damage: {equipmentItem.damageStat}   Defense: {equipmentItem.defenseStat}\nHealth: {equipmentItem.healthStat}   Move Speed: {equipmentItem.moveSpeedStat}";
            }
        }

        if (type == ItemType.Material)
        {
            MaterialItem materialItem = this as MaterialItem;
            if (materialItem != null)
            {
                return $"Value: {materialItem.itemDescription}";
            }
        }
        return "";
    }
}



public enum ItemType
{
    Equipment,
    Material
}

