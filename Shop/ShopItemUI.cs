using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] GameObject lockedTag, soldOutTag, prevLockedTag;
    [SerializeField] TextMeshProUGUI priceTag, requirementTag, nameTag;
    [SerializeField] Button myButton;
    public ShopItem myItem;
    public bool isLocked, isSoldOut, prevLocked;
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
        if (!isLocked) prevLockedTag.SetActive(prevLocked);
        soldOutTag.SetActive(isSoldOut);
        
        if (myButton) myButton.interactable = !isLocked && !isSoldOut && !prevLocked;
    }

    public void OnClick()
    {
        shop.TryPurchase(this);
    }
}
