using UnityEngine;
using System;

[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int quantity;

    public InventorySlot(ItemData itemData, int amount)
    {
        item = itemData;
        quantity = amount;
    }

    public void AddQuantity(int amount)
    {
        quantity += amount;
    }

    public void RemoveQuantity(int amount)
    {
        quantity -= amount;
        if (quantity < 0) quantity = 0;
    }

    public bool IsEmpty()
    {
        return item == null || quantity == 0;
    }

    public void ClearSlot()
    {
        item = null;
        quantity = 0;
    }
}