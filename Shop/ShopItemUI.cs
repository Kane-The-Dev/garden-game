using UnityEngine;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] GameObject lockedTag, soldOutTag;
    [SerializeField] TextMeshProUGUI priceTag, requirementTag, nameTag;
    public ShopItem myItem;
    public bool isLocked, isSoldOut;
    ShopManager shop;

    void Start()
    {
        shop = GameManager.instance.sm;
        Refresh();
    }

    public void Refresh()
    {
        priceTag.text = myItem.price.ToString() + "G";
        requirementTag.text = "Unlock at\nLvl. " + myItem.requirement.ToString();
        nameTag.text = myItem.itemName;

        lockedTag.SetActive(isLocked);
        soldOutTag.SetActive(isSoldOut);
    }

    public void OnClick()
    {
        shop.TryPurchase(this);
    }
}
