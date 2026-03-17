using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Material Item", menuName = "Items/Material Item")]

public class MaterialItem : Item
{
public enum MaterialType { Ore, Fragment }
public MaterialType materialType;

}
