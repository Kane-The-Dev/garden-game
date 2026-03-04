using UnityEngine;

public abstract class ShopItem : ScriptableObject
{
    public int requirement, price;
    public string itemName;

    public abstract void OnPurchase();
    public abstract int CanPurchase(Inventory inventory);
    // 0 = available, 1 = no exp, 2 = no money, 3 = no prev upgrade
}
