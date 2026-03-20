using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodButton : MonoBehaviour
{
    public int productID;
    public int sellPrice;

    Inventory inventory;
    EatingManager eater;

    void Start()
    {
        inventory = GameManager.instance.inventory;
        eater = GameManager.instance.em;
    }

    public void OnClick(int quantity)
    {
        if (inventory.foodList[productID].n < quantity) 
        {
            Debug.Log(inventory.foodList[productID].name + "out of stock" + inventory.foodList[productID].n);
            return;
        }

        if (eater.cooldownTimer > 0)
        {
            Debug.Log("Waiting for truck!");
            return;
        }

        if (eater.totalWeight + quantity * inventory.foodList[productID].weight > 30) 
        {
            Debug.Log("Truck is overloaded!");
            return;
        }

        for (int i = 0; i < quantity; i++)
            eater.q.Enqueue(productID);

        eater.totalWeight += quantity * inventory.foodList[productID].weight;
        eater.accumulatedStonks += quantity * inventory.foodList[productID].sellPrice;

        inventory.foodList[productID].UpdateN(-1 * quantity);
        inventory.UpdateStorage();
    }
}
