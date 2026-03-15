using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public List<Item> foodList = new();

    [Header("Display Shelves")]
    [SerializeField] List<ShopItemUI> buttons = new();
    Dictionary<ShopItem, int> stock = new();
    [SerializeField] Transform plantDisplay;
    [SerializeField] GameObject shopButton, displayBar, placeholderBar, displaySection;
    [SerializeField] RectTransform rootLayout;
    public RectTransform shopPanel;
    [SerializeField] ShopItem[] specialItems; // items that are tools but have infinite stock

    [Header("Display Board")]
    [SerializeField] TextMeshProUGUI stats;
    [SerializeField] TextMeshProUGUI itemName, itemDescription, itemPrice;
    [SerializeField] UIParticleSystem buyMessage;

    Inventory inventory;
    ShopItemUI selectedUI;

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
            newShopItem.description = item.description;

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

            bool newLocked = condition == 1;
            bool newPrevLocked = condition == 3;
            bool newSoldOut = stock[button.myItem] == 0;

            bool changed =
                button.isLocked != newLocked ||
                button.prevLocked != newPrevLocked ||
                button.isSoldOut != newSoldOut;

            if (changed)
            {
                button.isLocked = newLocked;
                button.prevLocked = newPrevLocked;
                button.isSoldOut = newSoldOut;
                button.Refresh();
            }
        }

        stats.text = "Lv. " + inventory.level + " | Bank: " + inventory.coin + "G";
    }

    void RefreshLayout()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rootLayout);
    }

    public void SetPurchase(ShopItemUI myUI)
    {
        selectedUI = myUI;
        itemName.text = myUI.myItem.itemName;
        itemPrice.text = myUI.myItem.price.ToString();
        itemDescription.text = myUI.myItem.description;
    }

    public void TryPurchase()
    {
        if (selectedUI == null) return;
        ShopItem myItem = selectedUI.myItem;

        if (myItem.CanPurchase(inventory) != 0)
        {
            Debug.Log("Cannot buy because of code " + myItem.CanPurchase(inventory));
            if (myItem.CanPurchase(inventory) == 2) buyMessage.Burst("Out of money!");
            return;
        }

        if (stock[myItem] <= 0)
        {
            Debug.Log("Out of stock!");
            buyMessage.Burst("Out of stock!");
            return;   
        }

        stock[myItem]--;
        buyMessage.Burst("+1");

        myItem.OnPurchase();
        RefreshShop();
    }
}