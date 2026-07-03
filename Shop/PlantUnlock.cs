using UnityEngine;

[CreateAssetMenu(menuName = "Shop Items/Plant")]
public class PlantUnlock : ShopItem
{
    // Inventory inventory;

    public override void OnPurchase(Inventory inventory)
    {
        if (!inventory.myInventory.ContainsKey(itemName))
            inventory.myInventory[itemName] = 0;

        inventory.myInventory[itemName]++;

        inventory.coin -= price;

        inventory.selection.RefreshPlants();
        inventory.selection.RefreshBuildings();

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
