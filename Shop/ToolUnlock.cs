using UnityEngine;

[CreateAssetMenu(menuName = "Shop Items/Tool")]
public class ToolUnlock : ShopItem
{
    [SerializeField] int toolType; // 1 = water, 2 = harvest, 3 = chop
    [SerializeField] GameObject myTool;
    [SerializeField] string prevUpgrade;
    // Inventory inventory;
    
    public override void OnPurchase(Inventory inventory)
    {
        if (!inventory.myInventory.ContainsKey(itemName))
            inventory.myInventory[itemName] = 0;

        inventory.myInventory[itemName]++;

        inventory.coin -= price;

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

        if (prevUpgrade != "None")
        {
            if (!inventory.myInventory.ContainsKey(prevUpgrade) || inventory.myInventory[prevUpgrade] <= 0)
                return 3;
        }
            
        return 0;
    }
}

