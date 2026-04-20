using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ListItem : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerClickHandler
{
    public Item item; // The item this ListItem represents
    public int quantity = 1;

    [Header("UI")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI quantityText;
    public GameObject selectionHighlight;

    public bool isSelected;

    public void Setup(Item item, int quantity = 1)
    {
        this.item = item;
        this.quantity = quantity;
        if (iconImage) iconImage.sprite = item.icon;
        if (nameText) nameText.text = item is RecipeItem recipe ? (!string.IsNullOrEmpty(recipe.recipeName) ? recipe.recipeName : item.itemName) : item.itemName;
        if (quantityText) quantityText.text = quantity > 1 ? quantity.ToString() : "";
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        // if (selectionHighlight) selectionHighlight.SetActive(selected);
        if (selected)
        {
            ForgeManager.Instance?.SetSelectedListItem(this);
            ForgeManager.Instance?.OnListItemSelected(this);
        }
    }

    public void OnSelect(BaseEventData eventData) => SetSelected(true);
    public void OnDeselect(BaseEventData eventData) => SetSelected(false);
    public void OnPointerClick(PointerEventData eventData) => SetSelected(true);
}
