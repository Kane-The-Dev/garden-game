using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Shop Items/Expansion")]
public class ExpansionUnlock : ShopItem
{
    Inventory inventory;

    public override void OnPurchase()
    {
        GameManager.instance.gd.UpgradeGarden();
        Debug.Log("You unlocked " + itemName);
    }

    public override int CanPurchase(Inventory inventory)
    {
        if (inventory.level < requirement) 
        {
            // Debug.Log("Insufficient level!");
            return 1;
        }

        if (inventory.coin < price)
        {
            // Debug.Log("Insufficient money!");
            return 2;
        }
            
        return 0;
    }
}
