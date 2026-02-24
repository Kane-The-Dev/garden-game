using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public List<Item> foodList = new();
    [SerializeField] List<ShopItemUI> buttons = new();
    Dictionary<ShopItem, int> stock = new();
    [SerializeField] Transform plantDisplay;
    [SerializeField] GameObject shopButton, displayBar, displaySection;
    [SerializeField] RectTransform shopPanel, rootLayout;
    Inventory inventory;

    void Start()
    {
        FindObjectOfType<ReadFile>().LoadItems(foodList);
        inventory = GameManager.instance.inventory;
        inventory.shop = this;
        InitializeShop();
        RefreshShop();
    }
    
    public void InitializeShop()
    {
        foreach (var upgrade in buttons) // default upgrade in stock = 1
        {
            stock[upgrade.myItem] = 1;
        }
        
        int count = 0;
        Transform thisRow = null;
        foreach (var item in foodList.OrderBy(f => f.levelReq)) // generate dynamic shop items for plants
        {
            if (count % 5 == 0)
            {
                Instantiate(displayBar, plantDisplay);
                GameObject newSection = Instantiate(displaySection, plantDisplay);
                thisRow = newSection.transform;
            }

            if (!thisRow) continue;
            GameObject newItem = Instantiate(shopButton, thisRow);
            
            PlantUnlock newShopItem = ScriptableObject.CreateInstance<PlantUnlock>();
            newShopItem.itemName = item.name;
            newShopItem.price = item.plantPrice;
            newShopItem.requirement = item.levelReq;

            stock[newShopItem] = 9999;

            ShopItemUI itemUI = newItem.GetComponent<ShopItemUI>();
            itemUI.isLocked = inventory.level < newShopItem.requirement;
            itemUI.isSoldOut = false;
            itemUI.myItem = newShopItem;

            buttons.Add(itemUI);
            count++;
        }

        RefreshShop();
    }

    public void OpenShop()
    {
        shopPanel.gameObject.SetActive(true);
        Invoke("RefreshLayout", 0.01f);
    }

    public void RefreshShop()
    {
        int currentLevel = inventory.level;

        foreach (ShopItemUI button in buttons)
        {
            bool locked = currentLevel < button.myItem.requirement;
            if (button.isLocked != locked)
            {
                button.isLocked = locked;
                button.Refresh();
            }
        }
    }

    void RefreshLayout()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rootLayout);
    }

    public void TryPurchase(ShopItemUI myUI)
    {
        ShopItem myItem = myUI.myItem;

        if (inventory.level < myItem.requirement) 
        {
            Debug.Log("Insufficient level!");
            return;
        }

        if (inventory.coin < myItem.price)
        {
            Debug.Log("Insufficient money!");
            return;
        }

        if (stock[myItem] <= 0)
        {
            Debug.Log("Out of stock!");
            return;   
        }

        stock[myItem]--;
        if (stock[myItem] == 0)
        {
            myUI.isSoldOut = true;
            myUI.Refresh();
        }

        myItem.OnPurchase();
    }
}