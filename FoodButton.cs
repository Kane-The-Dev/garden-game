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

    public void OnClick()
    {
        if(inventory.foodList[productID].n <= 0) return;

        inventory.coin += inventory.foodList[productID].sellPrice;
        inventory.foodList[productID].UpdateN(-1);
        inventory.UpdateStorage();
    }
}
