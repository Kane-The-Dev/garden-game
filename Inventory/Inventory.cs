using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Inventory : MonoBehaviour
{
    public List<Item> foodList = new List<Item>(), buildingList = new();
    public Dictionary<string, int> myInventory = new Dictionary<string, int>();

    [Header("Stats")]
    public int level;
    public float exp;
    public int coin;

    [Header("UI")]
    [SerializeField] TextMeshProUGUI coinDisplay, levelDisplay;
    [SerializeField] Slider expDisplay;
    [SerializeField] Transform storage;
    [SerializeField] GameObject foodItemPrefab;

    public ShopManager shop;
    public PlantSelection selection;

    [Header("Save and Load")]
    [SerializeField] string saveFileName = "InventorySave.txt";
    [SerializeField] bool saveToPersistentDataPath = true;

    public static string GetProductName(string name)
    {
        if (string.IsNullOrEmpty(name)) return string.Empty;
        if (name.EndsWith(" Seed", System.StringComparison.OrdinalIgnoreCase))
            return name.Substring(0, name.Length - 5);
        if (name.EndsWith(" Pack", System.StringComparison.OrdinalIgnoreCase))
            return name.Substring(0, name.Length - 5);
        return name;
    }

    void Awake()
    {
        FindObjectOfType<ReadFile>().LoadItems(foodList);
        FindObjectOfType<ReadFile>().LoadBuildings(buildingList);
    }

    void Start()
    {
        // Sync myInventory values to foodList at start
        foreach (var item in foodList)
        {
            string productName = GetProductName(item.name);
            if (myInventory.TryGetValue(productName, out int N))
                item.n = N;
            else
                myInventory[productName] = item.n;
        }

        UpdateStorage();
    }

    public void AddItemQuantity(string itemName, int amount)
    {
        if (string.IsNullOrEmpty(itemName)) return;

        if (!myInventory.ContainsKey(itemName))
            myInventory[itemName] = 0;
        
        myInventory[itemName] += amount;

        // Synchronize with foodList item.n if this is a food item
        string productName = GetProductName(itemName);
        var foodItem = foodList.FirstOrDefault(f => GetProductName(f.name) == productName);
        if (foodItem != null)
        {
            if (myInventory.TryGetValue(productName, out int productCount))
            {
                foodItem.n = productCount;
            }
        }
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

    public void UpdateStorage()
    {
        foreach (Transform child in storage)
        {
            Destroy(child.gameObject);
        }

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

            int qty = 0;
            if (myInventory.TryGetValue(displayName, out int countVal))
            {
                qty = countVal;
            }

            newItem.transform.GetChild(2)
                .GetComponent<TextMeshProUGUI>().text = qty + " left";
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
            lines.Add(entry.Key + "=" + entry.Value);
        }

        File.WriteAllLines(path, lines);
        Debug.Log("Inventory saved to: " + path);
    }
}
