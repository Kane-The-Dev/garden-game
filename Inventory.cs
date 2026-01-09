using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class Inventory : MonoBehaviour
{
    public List<Item> foodList = new List<Item>();
    Dictionary<string, int> inventory = new Dictionary<string, int>();
    [SerializeField] Transform storage;
    [SerializeField] GameObject foodItemPrefab;

    public int coin, level;
    public float exp;
    [SerializeField] TextMeshProUGUI coinDisplay, levelDisplay, expDisplay;

    void Awake()
    {
        FindObjectOfType<ReadFile>().LoadItems(foodList);
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
