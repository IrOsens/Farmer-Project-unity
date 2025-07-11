using UnityEngine;
using System.Collections.Generic;

public class HandheldItemController : MonoBehaviour
{
    [Header("Pengaturan Tangan")]
    [SerializeField] private Transform handPositionTransform;

    private ItemData currentItemInHandData;
    private GameObject currentHandheldObject;

    private KeyCode[] numberKeys = {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5,
        KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0
    };

    void Start()
    {
        if (handPositionTransform == null)
        {
            Debug.LogError("Referensi 'Hand Position Transform' belum diatur pada HandheldItemController!");
            enabled = false;
            return;
        }

        ClearHand();
    }

    void Update()
    {
        HandleNumberKeyInput();
    }

    private void HandleNumberKeyInput()
    {
        if (InventoryManager.Instance == null) return;

        for (int i = 0; i < numberKeys.Length; i++)
        {
            if (Input.GetKeyDown(numberKeys[i]))
            {
                if (i < InventoryManager.Instance.inventorySlots.Count)
                {
                    InventorySlot selectedSlot = InventoryManager.Instance.inventorySlots[i];
                    
                    if (selectedSlot.IsEmpty() || selectedSlot.item.handheldPrefab == null)
                    {
                        ClearHand();
                        Debug.Log($"Slot {i+1} kosong atau item tidak bisa dipegang.");
                        return;
                    }

                    if (currentItemInHandData == selectedSlot.item)
                    {
                        ClearHand();
                        Debug.Log($"Menyarungkan {selectedSlot.item.itemName}.");
                    }
                    else
                    {
                        EquipItem(selectedSlot.item);
                        Debug.Log($"Memegang {selectedSlot.item.itemName} dari slot {i+1}.");
                    }
                } else {
                    Debug.Log($"Slot {i+1} tidak ada dalam inventaris.");
                }
            }
        }
    }

    private void EquipItem(ItemData itemToEquip)
    {
        ClearHand();

        if (itemToEquip != null && itemToEquip.handheldPrefab != null)
        {
            currentHandheldObject = Instantiate(itemToEquip.handheldPrefab, handPositionTransform);
            currentHandheldObject.transform.localPosition = Vector3.zero;
            currentItemInHandData = itemToEquip;
            Debug.Log($"Item {itemToEquip.itemName} dipegang.");
        }
        else
        {
            Debug.LogWarning("Mencoba memegang item null atau item tanpa handheldPrefab.");
        }
    }

    public void ClearHand()
    {
        if (currentHandheldObject != null)
        {
            Destroy(currentHandheldObject);
            currentHandheldObject = null;
        }
        currentItemInHandData = null;
        Debug.Log("Tangan kosong.");
    }

    public ItemData GetCurrentItemInHand()
    {
        return currentItemInHandData;
    }
}