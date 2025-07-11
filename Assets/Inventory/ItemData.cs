using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Informasi Dasar Item")]
    public string itemName = "New Item";
    public Sprite icon;
    [TextArea(3, 5)]
    public string description = "Deskripsi item ini.";

    [Header("Properti Farming (Opsional, tergantung jenis item)")]
    public bool isSeed = false;
    public ItemData harvestResult;

    [Header("Visual di Tangan")]
    public GameObject handheldPrefab;

    [Header("Properti Ekonomi (Opsional)")]
    public int sellPrice = 10;
    public int buyPrice = 20;
}