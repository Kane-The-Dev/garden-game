using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Inventory : MonoBehaviour
{
    public List<Item> foodList = new List<Item>();
    Dictionary<string, int> inventory = new Dictionary<string, int>();
    public int coin, level;
    public float exp;
    [SerializeField] TextMeshProUGUI coinDisplay, levelDisplay, expDisplay;
    [SerializeField] Transform shop, storage;
    [SerializeField] GameObject plantItemPrefab, foodItemPrefab;

    void Awake()
    {
        GetComponent<ReadFile>().LoadItems(foodList);
        InitializeShop();
        UpdateStorage();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            foreach (var item in inventory)
            {
                Debug.Log(item.Key + " = " + item.Value);
            }
        }

        if (coinDisplay)
        coinDisplay.text = "Bank: " + coin.ToString() + "G";

        if (levelDisplay)
        levelDisplay.text = level.ToString();

        if (expDisplay)
        expDisplay.text = exp.ToString("F0") + "/100 EXP";

        exp += Time.deltaTime;
        if (exp >= 100f)
        {
            exp = 0f;
            level++;
        }
    }

    public void InitializeShop()
    {
        foreach (Transform child in shop)
        {
            Destroy(child.gameObject);
        }

        int count = 0;
        foreach (var item in foodList)
        {
            // Debug.Log(item.name + " = " + item.n);
            GameObject newItem = Instantiate(plantItemPrefab, shop);

            PlantButton thisButton = newItem.transform.GetChild(0).GetComponent<PlantButton>();
            shop.gameObject.GetComponent<ButtonGroup>().buttons.Add(thisButton.myImage);
            thisButton.myGroup = shop.gameObject.GetComponent<ButtonGroup>();
            
            thisButton.plantID = count++;
            thisButton.plantName = item.name;
            thisButton.plantPrice = item.plantPrice;
            
            if (item.levelReq == 0) newItem.transform.GetChild(1).gameObject.SetActive(false);
            newItem.transform.GetChild(1).GetComponent<UILock>().levelRequirement = item.levelReq;
        }
    }

    public void UpdateStorage()
    {
        foreach (Transform child in storage)
        {
            Destroy(child.gameObject);
        }

        int count = 0;
        foreach (var item in foodList)
        {
            // Debug.Log(item.name + " = " + item.n);
            GameObject newItem = Instantiate(foodItemPrefab, storage);
            newItem.GetComponent<FoodButton>().productID = count++;
            newItem.GetComponent<FoodButton>().sellPrice = item.sellPrice;
            newItem.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = item.name;
            newItem.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = item.n.ToString() + " left";
        }
    }
}
