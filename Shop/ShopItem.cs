using UnityEngine;

public abstract class ShopItem : ScriptableObject
{
    public int requirement, price;
    public string itemName;

    public abstract void OnPurchase();
}
