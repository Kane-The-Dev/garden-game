using UnityEngine;

[CreateAssetMenu(menuName = "Shop Items/Plant")]
public class PlantUnlock : ShopItem
{
    Inventory inventory;

    public override void OnPurchase()
    {
        inventory = GameManager.instance.inventory;

        if (!inventory.myInventory.ContainsKey(itemName))
            inventory.myInventory[itemName] = 0;

        inventory.myInventory[itemName]++;

        inventory.coin -= price;

        inventory.selection.RefreshPlants();

        Debug.Log("You unlocked " + itemName);
    }
}
