using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public List<Item> foodList = new();
    List<ShopItemUI> buttons = new();
    Dictionary<ShopItem, int> stock = new();
    [SerializeField] Transform shopDisplay;
    [SerializeField] GameObject shopButton;
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
        foreach (var item in foodList.OrderBy(f => f.levelReq))
        {
            GameObject newItem = Instantiate(shopButton, shopDisplay);
            
            PlantUnlock newShopItem = ScriptableObject.CreateInstance<PlantUnlock>();
            newShopItem.itemName = item.name;
            newShopItem.price = item.plantPrice;
            newShopItem.requirement = item.levelReq;

            stock[newShopItem] = 9999;

            ShopItemUI itemUI = newItem.GetComponent<ShopItemUI>();
            itemUI.myItem = newShopItem;
            itemUI.Refresh();

            buttons.Add(itemUI);
        }
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
}