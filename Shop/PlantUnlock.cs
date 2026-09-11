using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Shop Items/Plant")]
public class PlantUnlock : ShopItem
{
    public ItemType type;
    public bool isDecor = false;

    public override void OnPurchase(Inventory inventory, int quantity, int finalPrice = -1)
    {
        string key = itemName;

        inventory.AddItemQuantity(key, quantity, type);

        int cost = finalPrice >= 0 ? finalPrice : price * quantity;
        inventory.coin -= cost;

        inventory.selection.RefreshPlants();
        inventory.selection.RefreshBuildings();

        Debug.Log("You bought " + quantity + " " + key);
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
