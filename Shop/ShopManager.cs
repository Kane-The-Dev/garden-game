using System;
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
    [SerializeField] Transform plantDisplay, bakeryDisplay, buildDisplay;
    [SerializeField] GameObject shopPack, shopCard, displayBar, placeholderBar, displaySection;
    [SerializeField] RectTransform rootLayout;
    public RectTransform shopPanel;
    [SerializeField] ShopItem[] specialItems; // items that are tools but have infinite stock

    [Header("Display Board")]
    [SerializeField] TextMeshProUGUI stats;
    [SerializeField] TextMeshProUGUI itemName, itemDescription, itemPrice;
    [SerializeField] UIParticleSystem coinBurst;

    [Header("Other")]
    [SerializeField] AudioSource source;
    [SerializeField] AudioClip purchase, error;

    Inventory inventory;
    ShopItemUI selectedUI;
    GameManager gm;

    void Start()
    {
        gm = GameManager.instance;
        inventory = gm.inventory;
        inventory.shop = this;

        InitializeShop();
        RefreshShop();
    }

    PlantUnlock CreateShopItem(Item item)
    {
        PlantUnlock newShopItem = ScriptableObject.CreateInstance<PlantUnlock>();
        newShopItem.itemName = item.name;
        newShopItem.price = item.plantPrice;
        newShopItem.requirement = item.levelReq;
        newShopItem.description = item.description;

        stock[newShopItem] = 9999;

        return newShopItem;
    }

    void CreateShopItemUI(ShopItem myShopItem, GameObject prefab, Transform parent)
    {
        GameObject newItem = Instantiate(prefab, parent);

        ShopItemUI itemUI = newItem.GetComponent<ShopItemUI>();
        itemUI.isLocked = inventory.level < myShopItem.requirement;
        itemUI.isSoldOut = false;
        itemUI.myItem = myShopItem;

        buttons.Add(itemUI);
    }
    
    void InitializeShop()
    {
        foreach (var upgrade in buttons) // default tools in stock = 1
        {
            stock[upgrade.myItem] = 1;
        }
        
        int count = 0;
        Transform thisRow = null;

        // generate dynamic shop items for plants
        foreach (var item in inventory.foodList.OrderBy(f => f.levelReq)) 
        {
            if (item.type == "Other" || item.type == "Oven") continue;

            PlantUnlock newShopItem = CreateShopItem(item);

            if (count % 5 == 0)
            {
                GameObject subBar = Instantiate(placeholderBar, plantDisplay);
                GameObject mainBar = Instantiate(displayBar, plantDisplay.parent);
                mainBar.GetComponent<FollowPosition>().target = subBar.GetComponent<RectTransform>();

                GameObject newSection = Instantiate(displaySection, plantDisplay);
                thisRow = newSection.transform;
            }

            if (!thisRow) return;
            CreateShopItemUI(newShopItem, shopPack, thisRow);
            count++;
        }

        count = 0;
        thisRow = null;

        Item oven = inventory.buildingList.FirstOrDefault(item => item.name == "Oven");
        if (oven != null)
        {
            PlantUnlock newShopItem = CreateShopItem(oven);

            GameObject subBar = Instantiate(placeholderBar, bakeryDisplay);
            GameObject mainBar = Instantiate(displayBar, bakeryDisplay.parent);
            mainBar.GetComponent<FollowPosition>().target = subBar.GetComponent<RectTransform>();

            GameObject newSection = Instantiate(displaySection, bakeryDisplay);
            thisRow = newSection.transform;

            CreateShopItemUI(newShopItem, shopCard, thisRow);
            count++;
        }

        // generate dynamic shop items for baking goods
        foreach (var item in inventory.foodList.OrderBy(f => f.levelReq)) 
        {
            if (item.type == "Other" || item.type != "Oven") continue;

            PlantUnlock newShopItem = CreateShopItem(item);

            if (count % 5 == 0)
            {
                GameObject subBar = Instantiate(placeholderBar, bakeryDisplay);
                GameObject mainBar = Instantiate(displayBar, bakeryDisplay.parent);
                mainBar.GetComponent<FollowPosition>().target = subBar.GetComponent<RectTransform>();

                GameObject newSection = Instantiate(displaySection, bakeryDisplay);
                thisRow = newSection.transform;
            }

            if (!thisRow) return;
            CreateShopItemUI(newShopItem, shopPack, thisRow);
            count++;
        }

        count = 0;
        thisRow = null;

        // generate dynamic shop items for buildings
        foreach (var item in inventory.buildingList.OrderBy(f => f.levelReq)) 
        {
            if (item.type == "Other") continue;

            PlantUnlock newShopItem = CreateShopItem(item);

            if (count % 5 == 0)
            {
                GameObject subBar = Instantiate(placeholderBar, buildDisplay);
                GameObject mainBar = Instantiate(displayBar, buildDisplay.parent);
                mainBar.GetComponent<FollowPosition>().target = subBar.GetComponent<RectTransform>();

                GameObject newSection = Instantiate(displaySection, buildDisplay);
                thisRow = newSection.transform;
            }

            if (!thisRow) return;
            CreateShopItemUI(newShopItem, shopCard, thisRow);
            count++;
        }

        foreach (var item in specialItems) stock[item] = 9999;

        RefreshShop();
    }

    public void OpenShop()
    {
        // shopPanel.gameObject.SetActive(true);
        gm.UIAnimator.SetTrigger("openshop");
        Invoke("RefreshLayout", 0.01f);
    }

    public void CloseShop()
    {
        gm.UIAnimator.SetTrigger("closeshop");
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
            if (myItem.CanPurchase(inventory) == 2) 
                gm.mouse.myEffect.Burst("Out of money!");
            source.PlayOneShot(error);
            return;
        }

        if (stock[myItem] <= 0)
        {
            Debug.Log("Out of stock!");
            gm.mouse.myEffect.Burst("Out of stock!");
            source.PlayOneShot(error);
            return;   
        }

        if (myItem.price > 100f)
        {
            gm.AYSPanel.OpenPanel(
                "Do you want to buy " + myItem.itemName + " for " +myItem.price + "G?", 
                (result) => {
                    if (result)
                    {
                        stock[myItem]--;
                        gm.mouse.myEffect.Burst("+1");
                        coinBurst.Burst();
                        source.PlayOneShot(purchase);

                        myItem.OnPurchase(inventory);
                        RefreshShop();
                    }
                }
            );
        }
        else
        {
            stock[myItem]--;
            gm.mouse.myEffect.Burst("+1");
            coinBurst.Burst();
            source.PlayOneShot(purchase);

            myItem.OnPurchase(inventory);
            RefreshShop();
        }
    }
}