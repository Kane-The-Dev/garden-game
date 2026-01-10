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

        foreach (var item in foodList.OrderBy(f => f.levelReq))
        {
            GameObject newItem = Instantiate(plantItemPrefab, shop);

            PlantButton thisButton = newItem.transform.GetChild(0).GetComponent<PlantButton>();
            shop.gameObject.GetComponent<ButtonGroup>().buttons.Add(thisButton.myImage);
            thisButton.myGroup = shop.gameObject.GetComponent<ButtonGroup>();
            
            thisButton.plantID = item.ID;
            thisButton.plantName = item.name;
            thisButton.plantPrice = item.plantPrice;
            
            if (item.levelReq == 0) newItem.transform.GetChild(1).gameObject.SetActive(false);
            newItem.transform.GetChild(1).GetComponent<UILock>().levelRequirement = item.levelReq;
        }
    }
}
