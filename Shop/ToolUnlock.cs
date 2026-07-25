using UnityEngine;

[CreateAssetMenu(menuName = "Shop Items/Tool")]
public class ToolUnlock : ShopItem
{
    [SerializeField] int toolType; // 1 = water, 2 = harvest, 3 = chop
    [SerializeField] GameObject myTool;
    [SerializeField] string prevUpgrade;
    
    public override void OnPurchase(Inventory inventory, int quantity)
    {
        inventory.AddItemQuantity(itemName, quantity);

        inventory.coin -= price * quantity;

        for (int i = 0; i < quantity; i++)
        {
            var newTool = Instantiate(myTool, Vector3.zero, Quaternion.identity);
            switch (toolType)
            {
                case 1:
                    GameManager.instance.pm.waterTool = newTool.GetComponent<WaterTool>();
                    break;
                case 2:
                    GameManager.instance.pm.harvestTool = newTool.GetComponent<HarvestTool>();
                    break;
                case 3:
                    GameManager.instance.pm.chopTool = newTool.GetComponent<ChopTool>();
                    break;
            }
        }

        Debug.Log("You unlocked " + itemName);
    }

    public override int CanPurchase(Inventory inventory, int quantity)
    {
        if (inventory.level < requirement)
            return 1;

        if (inventory.coin < price * quantity)
            return 2;

        if (prevUpgrade != "None")
        {
            if (!inventory.myInventory.ContainsKey(prevUpgrade) || inventory.myInventory[prevUpgrade] <= 0)
                return 3;
        }

        return 0;
    }
}

