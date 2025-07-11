using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Pengaturan Inventaris")]
    [SerializeField] private int inventorySize = 10;
    public List<InventorySlot> inventorySlots;

    public delegate void OnInventoryChanged();
    public event OnInventoryChanged onInventoryChangedCallback;

    [Header("Item Awal (Untuk Debug/Testing)")]
    [SerializeField] private ItemData startingSeedItem;
    [SerializeField] private int startingSeedAmount = 1;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        inventorySlots = new List<InventorySlot>(inventorySize);
        for (int i = 0; i < inventorySize; i++)
        {
            inventorySlots.Add(new InventorySlot(null, 0));
        }

        if (startingSeedItem != null && startingSeedAmount > 0)
        {
            if (inventorySlots.Count > 0)
            {
                inventorySlots[0].item = startingSeedItem;
                inventorySlots[0].quantity = startingSeedAmount;
                Debug.Log($"Inventaris diisi dengan {startingSeedAmount}x {startingSeedItem.itemName} di slot 1.");
            }
            else
            {
                Debug.LogWarning("Inventaris tidak memiliki slot untuk diisi item awal.");
            }
        }

    }

    public bool AddItem(ItemData itemToAdd, int amount = 1)
    {
        if (itemToAdd == null || amount <= 0)
        {
            Debug.LogWarning("AddItem dipanggil dengan item atau jumlah yang tidak valid.");
            return false;
        }

        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.item == itemToAdd)
            {
                slot.AddQuantity(amount);
                Debug.Log($"Menambahkan {amount}x {itemToAdd.itemName} ke inventaris. Total: {slot.quantity}");
                onInventoryChangedCallback?.Invoke();
                return true;
            }
        }

        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.IsEmpty())
            {
                slot.item = itemToAdd;
                slot.quantity = amount;
                Debug.Log($"Menambahkan {amount}x {itemToAdd.itemName} ke slot baru.");
                onInventoryChangedCallback?.Invoke();
                return true;
            }
        }

        Debug.LogWarning($"Inventaris penuh! Tidak bisa menambahkan {itemToAdd.itemName}.");
        return false;
    }

    public bool RemoveItem(ItemData itemToRemove, int amount = 1)
    {
        if (itemToRemove == null || amount <= 0)
        {
            Debug.LogWarning("RemoveItem dipanggil dengan item atau jumlah yang tidak valid.");
            return false;
        }

        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.item == itemToRemove && slot.quantity >= amount)
            {
                slot.RemoveQuantity(amount);
                Debug.Log($"Menghapus {amount}x {itemToRemove.itemName} dari inventaris. Sisa: {slot.quantity}");
                if (slot.quantity == 0)
                {
                    slot.ClearSlot();
                }
                onInventoryChangedCallback?.Invoke();
                return true;
            }
        }

        Debug.LogWarning($"Tidak bisa menghapus {itemToRemove.itemName}. Item tidak ditemukan atau jumlah tidak mencukupi.");
        return false;
    }

    public bool HasItem(ItemData itemToCheck, int amount = 1)
    {
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.item == itemToCheck && slot.quantity >= amount)
            {
                return true;
            }
        }
        return false;
    }

    public int GetItemQuantity(ItemData itemToCheck)
    {
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.item == itemToCheck)
            {
                return slot.quantity;
            }
        }
        return 0;
    }
}