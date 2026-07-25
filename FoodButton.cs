using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodButton : MonoBehaviour
{
    public int productID;
    public int sellPrice;

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
        string productName = Inventory.GetProductName(inventory.foodList[productID].name);
        int productCount = 0;
        if (inventory.myInventory.TryGetValue(productName, out int count))
        {
            productCount = count;
        }

        if (productCount < quantity) 
        {
            gm.mouse.myEffect.Burst("Out of stock!");
            Debug.Log(productName + " out of stock " + productCount);
            return;
        }

        if (eater.cooldownTimer > 0f || !eater.myTruck || eater.myTruck.transform.position.y > 3f)
        {
            gm.mouse.myEffect.Burst("Waiting for Truck!");
            Debug.Log("Waiting for truck!");
            return;
        }

        if (eater.totalWeight + quantity * inventory.foodList[productID].weight > eater.maxWeight) 
        {
            gm.mouse.myEffect.Burst("Overloaded!");
            Debug.Log("Truck is overloaded!");
            return;
        }

        for (int i = 0; i < quantity; i++)
            eater.q.Enqueue(productID);

        eater.totalWeight += quantity * inventory.foodList[productID].weight;
        eater.accumulatedStonks += quantity * inventory.foodList[productID].sellPrice;

        inventory.AddItemQuantity(productName, -quantity);
        inventory.UpdateStorage();
    }
}
