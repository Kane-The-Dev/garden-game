using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Inventory : MonoBehaviour
{
    public List<Item> foodList = new List<Item>(), buildingList = new();
    public Dictionary<string, int> myInventory = new Dictionary<string, int>();
    [SerializeField] Transform storage;
    [SerializeField] GameObject foodItemPrefab;

    public int coin, level;
    int prevCoin;
    public float exp;
    [SerializeField] TextMeshProUGUI coinDisplay, levelDisplay;
    [SerializeField] Slider expDisplay;

    // [SerializeField] UIParticleSystem coinBurst;

    public ShopManager shop;
    public PlantSelection selection;

    void Start()
    {
        FindObjectOfType<ReadFile>().LoadItems(foodList);
        FindObjectOfType<ReadFile>().LoadBuildings(buildingList);
        UpdateStorage();
        prevCoin = coin;
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

        // if (prevCoin != coin) coinBurst.Burst();
        // prevCoin = coin;
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

            newItem.transform.GetChild(1)
                .GetComponent<TextMeshProUGUI>().text = item.name;

            newItem.transform.GetChild(2)
                .GetComponent<TextMeshProUGUI>().text = item.n + " left";
        }
    }
}
