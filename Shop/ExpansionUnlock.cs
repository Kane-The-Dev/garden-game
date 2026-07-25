using UnityEngine;

[CreateAssetMenu(menuName = "Shop Items/Expansion")]
public class ExpansionUnlock : ShopItem
{
    // Inventory inventory;

    public override void OnPurchase(Inventory inventory, int quantity)
    {
        inventory.coin -= price * quantity;

        for (int i = 0; i < quantity; i++)
        {
            GameManager.instance.fence.UpgradeFence();
        }
        
        Debug.Log("You unlocked " + itemName);
    }

    public override int CanPurchase(Inventory inventory, int quantity)
    {
        if (inventory.level < requirement)
            return 1;

        if (inventory.coin < price * quantity)
            return 2;

        return 0;
    }
}
