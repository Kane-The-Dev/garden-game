using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Shop Items/Plant")]
public class PlantUnlock : ShopItem
{
    public override void OnPurchase(Inventory inventory, int quantity)
    {
        string key = itemName;

        inventory.AddItemQuantity(key, quantity);

        inventory.coin -= price * quantity;

        inventory.selection.RefreshPlants();
        inventory.selection.RefreshBuildings();

        Debug.Log("You bought " + quantity + " " + key);
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
