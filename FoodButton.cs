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
        inventory = FindObjectOfType<Inventory>();
        eater = FindObjectOfType<EatingManager>();
    }

    public void OnClick(int quantity)
    {
        if (inventory.foodList[productID].n < quantity) {
            Debug.Log(inventory.foodList[productID].name + "out of stock" + inventory.foodList[productID].n);
            return;
        }

        for (int i = 0; i < quantity; i++)
        eater.q.Enqueue(productID);

        inventory.coin += quantity * inventory.foodList[productID].sellPrice;
        inventory.foodList[productID].UpdateN(-1 * quantity);
        inventory.UpdateStorage();
    }
}
