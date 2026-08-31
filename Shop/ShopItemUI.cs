using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] GameObject lockedTag, soldOutTag, prevLockedTag;
    [SerializeField] TextMeshProUGUI priceTag, requirementTag, nameTag;
    [SerializeField] Button myButton;
    [SerializeField] Image icon;
    public ShopItem myItem;
    public bool isLocked, isSoldOut, prevLocked;
    ShopManager shop;

    public void SetIcon(Sprite sprite)
    {
        if (sprite == null)
            return;

        if (icon != null)
        {
            icon.sprite = sprite;
            icon.enabled = true;
        }
    }

    void Start()
    {
        shop = GameManager.instance.sm;
        Refresh();

        if (!icon || !myItem) return;

        // Some items use the product icon (e.g. "Apple"), not the seed icon ("Apple Seed")
        string iconName = Inventory.GetProductName(myItem.itemName);
        SetIcon(ReadFile.LoadIconSprite(iconName));
    }

    public void Refresh()
    {
        priceTag.text = myItem.price.ToString() + "G";
        requirementTag.text = "Unlock at\nLvl. " + myItem.requirement.ToString();
        nameTag.text = myItem.itemName;

        lockedTag.SetActive(false);
        prevLockedTag.SetActive(false);
        soldOutTag.SetActive(false);

        if (isLocked)
            lockedTag.SetActive(true);
        else if (prevLocked)
            prevLockedTag.SetActive(true);
        else if (isSoldOut)
            soldOutTag.SetActive(true);

        if (myButton)
            myButton.interactable = !isLocked && !prevLocked && !isSoldOut;
    }

    public void OnClick()
    {
        if (myItem is not PlantUnlock) shop.quantity = 1;
        shop.SetPurchase(this);
    }
}
