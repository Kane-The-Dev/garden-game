using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ItemType
{
    plant,
    build,
    water,
    harvest,
    chop,
    none
}

[System.Serializable]
public struct InventoryEntry
{
    public int quantity;
    public ItemType type;
    public int slotID;

    public InventoryEntry(int quantity, ItemType type, int slotID = -1)
    {
        this.quantity = quantity;
        this.type = type;
        this.slotID = slotID;
    }
}

public class Inventory : MonoBehaviour
{
    public List<Item> foodList = new List<Item>(), buildingList = new();
    public Dictionary<string, InventoryEntry> myInventory = new();

    [Header("Stats")]
    public int level;
    public float exp, expToNextLvl = 100f;
    public int coin;

    [Header("UI")]
    [SerializeField] Slider expDisplay;
    [SerializeField] TextMeshProUGUI coinDisplay, levelDisplay;

    [Header("Dependencies")]
    public ShopManager shop;
    public PlantSelection selection;
    public InventoryDisplay myDisplay;
    public FoodStorage fs;
    [SerializeField] LevelUpTransition levelUp;

    [Header("Resources")]
    [SerializeField] string productsFolderPath = "Prefabs/Food";

    // Helper functions
    public static string GetProductName(string name)
    {
        if (string.IsNullOrEmpty(name)) return string.Empty;
        if (name.EndsWith(" Seed", System.StringComparison.OrdinalIgnoreCase))
            return name.Substring(0, name.Length - 5);
        if (name.EndsWith(" Pack", System.StringComparison.OrdinalIgnoreCase))
            return name.Substring(0, name.Length - 5);
        return name;
    }

    public int GetQuantity(string name)
    {
        return myInventory.TryGetValue(name, out InventoryEntry e) ? e.quantity : 0;
    }

    public ItemType GetItemType(string name)
    {
        if (string.IsNullOrEmpty(name)) return ItemType.none;

        if (myInventory.TryGetValue(name, out InventoryEntry e) && e.type != ItemType.none)
            return e.type;

        foreach (var item in foodList)
            if (item.name == name) return ItemType.plant;

        foreach (var item in buildingList)
            if (item.name == name) return ItemType.build;

        return ItemType.none;
    }

    void Awake()
    {
        ReadFile.LoadItems(foodList);
        ReadFile.LoadBuildings(buildingList);
    }

    void Start()
    {
        // Initialize both keys in myInventory so they always exist from the start
        foreach (var item in foodList)
        {
            string productName = GetProductName(item.name);

            // Product key ("Apple") - increment on harvest, decrement on sale
            if (!myInventory.ContainsKey(productName))
                myInventory[productName] = new InventoryEntry(0, ItemType.none);

            // Seed/Pack key ("Apple Seed") - increment on purchase, decrement on planting
            if (item.name != productName && !myInventory.ContainsKey(item.name))
                myInventory[item.name] = new InventoryEntry(0, ItemType.plant);
        }
    }

    public void AddItemQuantity(string itemName, int amount, ItemType type = ItemType.none)
    {
        if (string.IsNullOrEmpty(itemName)) return;

        bool found = myInventory.TryGetValue(itemName, out InventoryEntry existing);
        int slotID = found ? existing.slotID : -1;
        int newQuantity = (found ? existing.quantity : 0) + amount;
        ItemType newType = type != ItemType.none ? type : (found ? existing.type : ItemType.none);

        if (slotID > -1)
        {
            // Already assigned to a slot -> refresh that slot
            int newSlotID = newQuantity > 0 ? slotID : -1;
            InventoryEntry updated = new InventoryEntry(newQuantity, newType, newSlotID);
            myInventory[itemName] = updated;
            myDisplay.RefreshSlot(slotID, itemName, updated);
        }
        else
        {
            // No slot yet -> Find an empty slot to assign
            int emptySlot = myDisplay.FindEmptySlot();
            if (emptySlot >= 0)
            {
                myDisplay.PlaceItemAtSlot(emptySlot, itemName, newType);
                InventoryEntry updated = new InventoryEntry(newQuantity, newType, emptySlot);
                myInventory[itemName] = updated;
                myDisplay.RefreshSlot(emptySlot, itemName, updated);
            }
            else
            {
                myInventory[itemName] = new InventoryEntry(newQuantity, newType, -1);
                Debug.LogWarning($"Inventory is full, cannot display: {itemName}");
            }
        }
    }

    void Update()
    {
        if (coinDisplay)
            coinDisplay.text = coin.ToString() + "G";

        if (levelDisplay)
            levelDisplay.text = level.ToString();

        if (expDisplay)
            expDisplay.value = exp / expToNextLvl;

        exp += Time.deltaTime * GameManager.instance.timeControl;
        if (exp >= expToNextLvl)
        {
            exp = 0f;
            level++;
            expToNextLvl = 100f + level * 10f;

            shop.RefreshShop();
            levelUp.LevelUp(level);
        }
    }

    public GameObject LoadProductPrefab(string itemName)
    {
        string productName = GetProductName(itemName);
        return ReadFile.LoadPrefab(productName, productsFolderPath);
    }
}
