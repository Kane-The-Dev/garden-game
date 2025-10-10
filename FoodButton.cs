using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodButton : MonoBehaviour
{
    public int productID;
    public int sellPrice;
    Inventory inventory;

    void Start()
    {
        inventory = FindObjectOfType<Inventory>();
    }

    public void OnClick(int quantity)
    {
        if (inventory.foodList[productID].n < quantity) return;

        for (int i = 0; i < quantity; i++)
        {
            Debug.Log("Foood");
        }

        inventory.coin += quantity * inventory.foodList[productID].sellPrice;
        inventory.foodList[productID].UpdateN(-1 * quantity);
        inventory.UpdateStorage();
    }
}
