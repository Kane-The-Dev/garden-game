using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public List<Item> foodList = new List<Item>();
    [SerializeField] Transform shop;
    [SerializeField] GameObject plantItemPrefab;

    void Awake()
    {
        FindObjectOfType<ReadFile>().LoadItems(foodList);
        InitializeShop();
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

        // Sort shop theo levelRequirement
        List<Transform> items = new List<Transform>();
        foreach (Transform child in shop)
        {
            items.Add(child);
        }
        
        items = items.OrderBy(item =>
            item.GetChild(1)
                .GetComponent<UILock>()
                .levelRequirement
        ).ToList();

        for (int i = 0; i < items.Count; i++)
        {
            items[i].SetSiblingIndex(i);
        }
    }
}
