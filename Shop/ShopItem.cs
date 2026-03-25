using UnityEngine;

public abstract class ShopItem : ScriptableObject
{
    public int requirement, price;
    public string itemName, description;

    public abstract void OnPurchase(Inventory inventory);
    public abstract int CanPurchase(Inventory inventory);
    // 0 = available, 1 = no exp, 2 = no money, 3 = no prev upgrade
}
