using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class Slot : MonoBehaviour
{
    public int SlotID { get; private set; }

    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Image iconImage;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
    }

    public void Initialize(int ID, InventoryDisplay manager)
    {
        SlotID = ID;
        button.onClick.AddListener(() => manager.SelectSlot(SlotID));
    }

    public void SetQuantity(int quantity)
    {
        if (quantityText == null) return;

        // Hide the number entirely for empty slots
        quantityText.text = quantity > 0 ? quantity.ToString() : string.Empty;
    }

    public void SetIcon(Sprite sprite)
    {
        if (iconImage == null)
            return;

        iconImage.sprite = sprite;
        iconImage.preserveAspect = true;
        iconImage.enabled = sprite != null;
    }
}