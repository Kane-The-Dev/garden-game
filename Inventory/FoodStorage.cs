using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class FoodStorage : MonoBehaviour
{
    List<Item> foodList = new List<Item>(), buildingList = new();
    [SerializeField] Transform storage;
    [SerializeField] GameObject foodItemPrefab;
    Inventory myInventory;

    void Start()
    {
        myInventory = GameManager.instance.inventory;
        foodList = myInventory.foodList;
        UpdateStorage();
    }

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

            string displayName = Inventory.GetProductName(item.name);
            fb.myName.text = displayName;

            int n = myInventory.GetQuantity(displayName);
            fb.myQuantity.text = n + " left";

            Sprite icon = ReadFile.LoadIconSprite(displayName);
            fb.myIcon.sprite = icon;
        }
    }
}
