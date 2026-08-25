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
    public int quantity = 1;
    [SerializeField] GameObject quantityOption, buyOption;
    [SerializeField] TextMeshProUGUI stats, itemName, itemDescription;
    [SerializeField] TextMeshProUGUI itemPrice, quantityDisplay;
    [SerializeField] UIParticleSystem coinBurst;
    [SerializeField] GameObject plantStats;
    [SerializeField] Slider growSpeed, sellPrice, weight;
    [SerializeField] TextMeshProUGUI growSpeedText;
    float maxGS, maxP, maxW;

    [Header("Audio")]
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

    Transform CreateNewRow(Transform display)
    {
        GameObject subBar = Instantiate(placeholderBar, display);
        GameObject mainBar = Instantiate(displayBar, display.parent.parent);
        FollowTransform follow = mainBar.GetComponent<FollowTransform>();
        follow.isRect = true;
        follow.rectTarget = subBar.GetComponent<RectTransform>();
        follow.positionSpeed = 0f;

        GameObject newSection = Instantiate(displaySection, display);
        return newSection.transform;
    }
    
    void InitializeShop()
    {
        maxGS = 0f;
        maxP = 0f;
        maxW = 0f;

        if (inventory != null)
        {
            foreach (var item in inventory.foodList)
            {
                if (item.growthSpeed > maxGS) maxGS = item.growthSpeed;
                if (item.sellPrice > maxP) maxP = item.sellPrice;
                if (item.weight > maxW) maxW = item.weight;
            }

            foreach (var item in inventory.buildingList)
            {
                if (item.growthSpeed > maxGS) maxGS = item.growthSpeed;
                if (item.sellPrice > maxP) maxP = item.sellPrice;
                if (item.weight > maxW) maxW = item.weight;
            }
        }

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
            newShopItem.type = ItemType.plant;

            if (count % 5 == 0)
                thisRow = CreateNewRow(plantDisplay);

            if (!thisRow) return;
            CreateShopItemUI(newShopItem, shopPack, thisRow);
            count++;
        }

        count = 0;
        thisRow = null;
        
        // generate shop item for Oven
        Item oven = inventory.buildingList.FirstOrDefault(item => item.name == "Oven");
        if (oven != null)
        {
            PlantUnlock newShopItem = CreateShopItem(oven);
            newShopItem.type = ItemType.build;

            thisRow = CreateNewRow(bakeryDisplay);

            CreateShopItemUI(newShopItem, shopCard, thisRow);
            count++;
        }

        // generate dynamic shop items for baking goods
        foreach (var item in inventory.foodList.OrderBy(f => f.levelReq)) 
        {
            if (item.type == "Other" || item.type != "Oven") continue;

            PlantUnlock newShopItem = CreateShopItem(item);
            newShopItem.type = ItemType.plant;

            if (count % 5 == 0)
                thisRow = CreateNewRow(bakeryDisplay);

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
            newShopItem.type = ItemType.build;

            if (count % 5 == 0)
                thisRow = CreateNewRow(buildDisplay);

            if (!thisRow) return;
            CreateShopItemUI(newShopItem, shopCard, thisRow);
            count++;
        }

        foreach (var item in specialItems) stock[item] = 9999;

        RefreshShop();
    }

    public void OpenShop()
    {
        gm.UIAnimator.SetTrigger("openshop");
        gm.cam.movable = false;
        Invoke("RefreshLayout", 0.05f);
    }

    public void CloseShop()
    {
        gm.UIAnimator.SetTrigger("closeshop");
        gm.cam.movable = true;
    }

    public void RefreshShop()
    {
        foreach (ShopItemUI button in buttons)
        {
            int condition = button.myItem.CanPurchase(inventory, quantity);

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

    public void QuantityUp()
    {
        if (quantity < 10)
        {
            quantity++;
            SetPurchase(selectedUI);
        }
    }

    public void QuantityDown()
    {
        if (quantity > 1)
        {
            quantity--;
            SetPurchase(selectedUI);
        }   
    }

    public void SetPurchase(ShopItemUI myUI)
    {
        if (!myUI) return;
        selectedUI = myUI;
        
        itemName.text = myUI.myItem.itemName;
        itemPrice.text = (myUI.myItem.price * quantity).ToString();
        itemDescription.text = myUI.myItem.description;

        buyOption.SetActive(true);

        if (stock[myUI.myItem] <= 1)
            quantityOption.SetActive(false);
        else
            quantityOption.SetActive(true);
        quantityDisplay.text = quantity.ToString();

        // Update stats sliders based on the product name matching inside the inventory lists
        string productName = Inventory.GetProductName(myUI.myItem.itemName);
        Item matchingItem = null;
        if (inventory != null)
        {
            matchingItem = inventory.foodList.FirstOrDefault(item => 
                Inventory.GetProductName(item.name).Equals(productName, StringComparison.OrdinalIgnoreCase)
            );
        }

        if (matchingItem != null)
        {
            if (plantStats) plantStats.SetActive(true);
            
            if (growSpeed) growSpeed.value = matchingItem.growthSpeed / maxGS;
            if (sellPrice) sellPrice.value = (float)matchingItem.sellPrice / maxP;
            if (weight) weight.value = matchingItem.weight / maxW;

            if (matchingItem.type == "Oven") growSpeedText.text = "Bake Speed";
            else growSpeedText.text = "Grow Speed";
        }
        else if (plantStats) plantStats.SetActive(false);
    }

    public void TryPurchase()
    {
        if (selectedUI == null) return;

        ShopItem myItem = selectedUI.myItem;
        int condition = myItem.CanPurchase(inventory, quantity);
        int totalPrice = myItem.price * quantity;

        if (condition != 0)
        {
            Debug.Log("Cannot buy because of code " + condition);
            if (condition == 2)
                gm.mouse.myEffect.Burst("Out of money!");

            source.PlayOneShot(error);
            return;
        }

        if (stock[myItem] < quantity)
        {
            Debug.Log("Out of stock!");
            gm.mouse.myEffect.Burst("Out of stock!");
            source.PlayOneShot(error);
            return;
        }

        void Purchase()
        {
            stock[myItem] -= quantity;
            gm.mouse.myEffect.Burst("+" + quantity);
            coinBurst.Burst();
            source.PlayOneShot(purchase);

            myItem.OnPurchase(inventory, quantity);

            RefreshShop();
        }

        if (totalPrice >= 300)
        {
            gm.AYSPanel.OpenPanel(
                "Do you want to buy " + quantity + " " + myItem.itemName + " for " + totalPrice + "G?",
                (result) => { if (result) Purchase(); }
            );
        }
        else Purchase();
    }
}
