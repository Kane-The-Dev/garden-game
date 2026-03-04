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
    [SerializeField] GameObject shopButton, displayBar, placeholderBar, displaySection;
    [SerializeField] RectTransform shopPanel, rootLayout;
    [SerializeField] ShopItem[] specialItems; // items that are tools but have infinite stock
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
        foreach (var upgrade in buttons) // default tools in stock = 1
        {
            stock[upgrade.myItem] = 1;
        }
        
        int count = 0;
        Transform thisRow = null;
        foreach (var item in foodList.OrderBy(f => f.levelReq)) // generate dynamic shop items for plants
        {
            if (item.type == "Other") continue;

            PlantUnlock newShopItem = ScriptableObject.CreateInstance<PlantUnlock>();
            newShopItem.itemName = item.name;
            newShopItem.price = item.plantPrice;
            newShopItem.requirement = item.levelReq;

            stock[newShopItem] = 9999;

            if (count % 5 == 0)
            {
                GameObject subBar = Instantiate(placeholderBar, plantDisplay);
                GameObject mainBar = Instantiate(displayBar, plantDisplay.parent);
                mainBar.GetComponent<FollowPosition>().target = subBar.GetComponent<RectTransform>();

                GameObject newSection = Instantiate(displaySection, plantDisplay);
                thisRow = newSection.transform;
            }

            if (!thisRow) continue;
            GameObject newItem = Instantiate(shopButton, thisRow);

            ShopItemUI itemUI = newItem.GetComponent<ShopItemUI>();
            itemUI.isLocked = inventory.level < newShopItem.requirement;
            itemUI.isSoldOut = false;
            itemUI.myItem = newShopItem;

            buttons.Add(itemUI);
            count++;
        }

        foreach (var item in specialItems) stock[item] = 9999;

        RefreshShop();
    }

    public void OpenShop()
    {
        shopPanel.gameObject.SetActive(true);
        Invoke("RefreshLayout", 0.01f);
    }

    public void RefreshShop()
    {
        foreach (ShopItemUI button in buttons)
        {
            int condition = button.myItem.CanPurchase(inventory);
            
            if (button.isLocked != (condition == 1))
            {
                button.isLocked = condition == 1;
                button.Refresh();
            }

            if (button.prevLocked != (condition == 3))
            {
                button.prevLocked = condition == 3;
                button.Refresh();
            }

            if (stock[button.myItem] == 0)
            {
                button.isSoldOut = true;
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

        if (myItem.CanPurchase(inventory) != 0)
        {
            Debug.Log("Cannot buy because of code " + myItem.CanPurchase(inventory));
            return;
        }

        if (stock[myItem] <= 0)
        {
            Debug.Log("Out of stock!");
            return;   
        }

        stock[myItem]--;

        myItem.OnPurchase();
        RefreshShop();
    }
}