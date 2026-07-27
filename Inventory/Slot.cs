using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Slot : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    public int SlotID { get; private set; }
    public Button button;
    public Image iconImage;
    public ItemType type = ItemType.none;
    public int n = 0; // quantity
    public string itemName = string.Empty;
    [SerializeField] TextMeshProUGUI quantityText;

    InventoryDisplay manager;

    void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
    }

    public void Initialize(int ID, InventoryDisplay manager)
    {
        SlotID = ID;
        this.manager = manager;
        button.onClick.AddListener(() => manager.SelectSlot(SlotID));
        button.onClick.AddListener(() => manager.myGroup.OnClick(button.gameObject));
    }

    // Update Info

    public void SetItem(string name, ItemType itemType)
    {
        itemName = name;
        type = itemType;
    }

    public void ClearItem()
    {
        itemName = string.Empty;
        type = ItemType.none;
    }

    public void SetQuantity(int quantity)
    {
        if (quantityText == null) return;

        // Hide the number for empty slots
        quantityText.text = quantity > 0 ? quantity.ToString() : string.Empty;
        n = quantity;
    }

    public void SetIcon(Sprite sprite)
    {
        if (iconImage == null)
            return;

        iconImage.sprite = sprite;
        iconImage.preserveAspect = true;
        iconImage.enabled = sprite != null;
    }

    // Drag and Drop

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (iconImage.sprite == null || manager == null) return;
        manager.BeginDrag(SlotID, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (manager == null) return;
        manager.UpdateDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (manager == null) return;
        manager.EndDrag(SlotID);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (manager == null) return;
        manager.SetHoveredSlot(SlotID);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (manager == null) return;
        manager.ClearHoveredSlot(SlotID);
    }
}