using UnityEngine;

[CreateAssetMenu(menuName = "Shop Items/Expansion")]
public class ExpansionUnlock : ShopItem
{
    // Inventory inventory;

    public override void OnPurchase(Inventory inventory, int quantity, int finalPrice = -1)
    {
        int cost = finalPrice >= 0 ? finalPrice : price * quantity;
        inventory.coin -= cost;

        for (int i = 0; i < quantity; i++)
        {
            GameManager.instance.fence.UpgradeFence();
        }
        
        Debug.Log("You unlocked " + itemName);
    }

    public override int CanPurchase(Inventory inventory, int quantity, int finalPrice = -1)
    {
        if (inventory.level < requirement)
            return 1;

        int cost = finalPrice >= 0 ? finalPrice : price * quantity;
        if (inventory.coin < cost)
            return 2;

        return 0;
    }
}
