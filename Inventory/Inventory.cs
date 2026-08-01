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

    public InventoryEntry(int quantity, ItemType type)
    {
        this.quantity = quantity;
        this.type = type;
    }
}

public class Inventory : MonoBehaviour
{
    public List<Item> foodList = new List<Item>(), buildingList = new();
    public Dictionary<string, InventoryEntry> myInventory = new();

    [Header("Stats")]
    public int level;
    public float exp;
    public int coin;

    [Header("UI")]
    [SerializeField] Slider expDisplay;
    [SerializeField] TextMeshProUGUI coinDisplay, levelDisplay;
    [SerializeField] Transform storage;
    [SerializeField] GameObject foodItemPrefab;

    public ShopManager shop;
    public PlantSelection selection;
    public InventoryDisplay myDisplay;

    [Header("Resources")]
    [SerializeField] string productsFolderPath = "Prefabs/Food";

    [Header("Save and Load")]
    [SerializeField] string saveFileName = "InventorySave.txt";
    [SerializeField] bool saveToPersistentDataPath = true;

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

    public int GetQuantity(string name)
    {
        return myInventory.TryGetValue(name, out InventoryEntry e) ? e.quantity : 0;
    }

    void Awake()
    {
        FindObjectOfType<ReadFile>().LoadItems(foodList);
        FindObjectOfType<ReadFile>().LoadBuildings(buildingList);
    }

    void Start()
    {
        // Initialize both keys in myInventory so they always exist from the start
        foreach (var item in foodList)
        {
            string productName = GetProductName(item.name);

            // Product key ("Apple") — increment on harvest, decrement on sale
            if (!myInventory.ContainsKey(productName))
                myInventory[productName] = new InventoryEntry(0, ItemType.none);

            // Seed/Pack key ("Apple Seed") — increment on purchase, decrement on planting
            if (item.name != productName && !myInventory.ContainsKey(item.name))
                myInventory[item.name] = new InventoryEntry(0, ItemType.plant);
        }

        UpdateStorage();
    }

    public void AddItemQuantity(string itemName, int amount, ItemType type = ItemType.none)
    {
        if (string.IsNullOrEmpty(itemName)) return;

        if (myInventory.TryGetValue(itemName, out InventoryEntry result))
        {
            myInventory[itemName] = new InventoryEntry(
                result.quantity + amount,
                type != ItemType.none ? type : result.type
            );
        }
        else
            myInventory[itemName] = new InventoryEntry(amount, type);

        myDisplay.Refresh(myInventory);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            SaveInventoryToFile();

        if (coinDisplay)
            coinDisplay.text = coin.ToString() + "G";

        if (levelDisplay)
            levelDisplay.text = level.ToString();

        if (expDisplay)
            expDisplay.value = exp / 100f;

        exp += Time.deltaTime * GameManager.instance.timeControl;
        if (exp >= 100f)
        {
            exp = 0f;
            level++;
            shop.RefreshShop();
        }
    }

    // Food Storage
    
    public void UpdateStorage()
    {
        foreach (Transform child in storage)
            Destroy(child.gameObject);

        foreach (var item in foodList.OrderBy(f => f.levelReq))
        {
            if (item.type == "Other") continue;
            
            GameObject newItem = Instantiate(foodItemPrefab, storage);
            var fb = newItem.GetComponent<FoodButton>();
            fb.productID = item.ID;
            fb.sellPrice = item.sellPrice;

            string displayName = GetProductName(item.name);
            newItem.transform.GetChild(1)
                .GetComponent<TextMeshProUGUI>().text = displayName;

            int n = GetQuantity(displayName);

            newItem.transform.GetChild(2)
                .GetComponent<TextMeshProUGUI>().text = n + " left";
        }
    }

    public void SaveInventoryToFile()
    {
        if (myInventory == null)
            return;

        string path = saveToPersistentDataPath
            ? Path.Combine(Application.persistentDataPath, saveFileName)
            : Path.Combine(Application.dataPath, saveFileName);

        List<string> lines = new List<string>();
        foreach (var entry in myInventory.OrderBy(e => e.Key))
        {
            lines.Add(entry.Key + "=" + entry.Value.quantity);
        }

        File.WriteAllLines(path, lines);
        Debug.Log("Inventory saved to: " + path);
    }

    public GameObject LoadProductPrefab(string itemName)
    {
        if (string.IsNullOrEmpty(itemName))
            return null;

        string productName = GetProductName(itemName);

        string resourcePath = string.IsNullOrEmpty(productsFolderPath)
            ? productName
            : productsFolderPath + "/" + productName;

        GameObject prefab = Resources.Load<GameObject>(resourcePath);

        if (prefab == null)
        {
            string fallbackPath = string.IsNullOrEmpty(productsFolderPath)
                ? productName.Replace(" ", string.Empty)
                : productsFolderPath + "/" + productName.Replace(" ", string.Empty);

            prefab = Resources.Load<GameObject>(fallbackPath);
        }

        if (prefab == null)
            Debug.LogWarning($"No product prefab found for '{itemName}' in Resources/{productsFolderPath}");

        return prefab;
    }
}
