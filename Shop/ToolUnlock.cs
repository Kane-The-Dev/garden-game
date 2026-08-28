using UnityEngine;

[CreateAssetMenu(menuName = "Shop Items/Tool")]
public class ToolUnlock : ShopItem
{
    [SerializeField] int toolType; // 1 = water, 2 = harvest, 3 = chop
    [SerializeField] GameObject myTool;
    [SerializeField] string prevUpgrade;
    
    public override void OnPurchase(Inventory inventory, int quantity)
    {
        // Map toolType int → ItemType enum
        ItemType itemType = toolType switch
        {
            1 => ItemType.water,
            2 => ItemType.harvest,
            3 => ItemType.chop,
            _ => ItemType.none
        };

        inventory.AddItemQuantity(itemName, quantity, itemType);
        inventory.coin -= price * quantity;

        SpawnToolObjects(quantity);

        Debug.Log("You unlocked " + itemName);
    }

    // Restores tools after a save load without affecting inventory
    public void RestoreTool(int quantity)
    {
        SpawnToolObjects(quantity);
        Debug.Log($"Restored tool: {itemName}");
    }

    void SpawnToolObjects(int quantity)
    {
        for (int i = 0; i < quantity; i++)
        {
            var newTool = Instantiate(myTool, Vector3.zero, Quaternion.identity);
            switch (toolType)
            {
                case 1:
                    GameManager.instance.pm.myWaterTools[itemName] = newTool;
                    break;
                case 2:
                    GameManager.instance.pm.myHarvestTools[itemName] = newTool;
                    break;
                case 3:
                    GameManager.instance.pm.myChopTools[itemName] = newTool;
                    break;
            }
        }
    }

    public override int CanPurchase(Inventory inventory, int quantity)
    {
        if (inventory.level < requirement)
            return 1;

        if (prevUpgrade != "None")
        {
            if (inventory.GetQuantity(prevUpgrade) <= 0)
                return 3;
        }

        if (inventory.coin < price * quantity)
            return 2;

        return 0;
    }
}

