using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class Inventory : MonoBehaviour
{
    public List<Item> foodList = new List<Item>();
    public Dictionary<string, int> myInventory = new Dictionary<string, int>();
    [SerializeField] Transform storage;
    [SerializeField] GameObject foodItemPrefab;

    public int coin, level;
    public float exp;
    [SerializeField] TextMeshProUGUI coinDisplay, levelDisplay, expDisplay;

    public ShopManager shop;
    public PlantSelection selection;

    void Awake()
    {
        FindObjectOfType<ReadFile>().LoadItems(foodList);
        UpdateStorage();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            foreach (var item in myInventory)
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
            GameObject newItem = Instantiate(foodItemPrefab, storage);
            var fb = newItem.GetComponent<FoodButton>();
            fb.productID = item.ID;
            fb.sellPrice = item.sellPrice;

            newItem.transform.GetChild(1)
                .GetComponent<TextMeshProUGUI>().text = item.name;

            newItem.transform.GetChild(2)
                .GetComponent<TextMeshProUGUI>().text = item.n + " left";
        }
    }
}
