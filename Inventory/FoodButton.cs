using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FoodButton : MonoBehaviour
{
    public int productID;
    public int sellPrice;
    public bool isGolden;

    public TextMeshProUGUI myName, myQuantity;
    public Image myIcon;

    GameManager gm;
    Inventory inventory;
    EatingManager eater;

    void Start()
    {
        gm = GameManager.instance;
        inventory = gm.inventory;
        eater = gm.em;
    }

    public void OnClick(int quantity)
    {
        if (!gm) gm = GameManager.instance;
        if (!inventory) inventory = gm ? gm.inventory : FindObjectOfType<Inventory>();
        if (!eater) eater = gm ? gm.em : FindObjectOfType<EatingManager>();

        if (!inventory || !gm || !eater) return;

        string productName = Inventory.GetProductName(inventory.foodList[productID].name);
        if (isGolden) productName = "Golden " + productName;
        int productCount = inventory.GetQuantity(productName);

        if (productCount < quantity) 
        {
            gm.mouse.myEffect.Burst("Out of stock!", new Color32(0xDE, 0x55, 0x57, 0xFF));
            gm.am.PlayUISoundEffect(6);
            return;
        }

        if (eater.cooldownTimer > 0f || !eater.myTruck || eater.myTruck.transform.position.y > 3f)
        {
            gm.mouse.myEffect.Burst("Truck unavailable!", new Color32(0xDE, 0x55, 0x57, 0xFF));
            gm.am.PlayUISoundEffect(6);
            return;
        }

        if (eater.totalWeight + quantity * inventory.foodList[productID].weight > eater.maxWeight.Value) 
        {
            gm.mouse.myEffect.Burst("Overloaded!", new Color32(0xDE, 0x55, 0x57, 0xFF));
            gm.am.PlayUISoundEffect(6);
            return;
        }

        int finalSellPrice = isGolden ? inventory.foodList[productID].sellPrice * 2 : inventory.foodList[productID].sellPrice;

        for (int i = 0; i < quantity; i++)
            eater.q.Enqueue(new FoodDropRequest(productID, isGolden));

        eater.totalWeight += quantity * inventory.foodList[productID].weight;
        if (inventory.foodList[productID].type != "Oven")
            eater.accumulatedStonks_P += quantity * finalSellPrice;
        else
            eater.accumulatedStonks_O += quantity * finalSellPrice;
        
        eater.CalculateBonus();

        inventory.AddItemQuantity(productName, -quantity);
        inventory.fs.UpdateStorage();
    }
}
