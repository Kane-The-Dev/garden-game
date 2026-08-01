using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventoryDisplay : MonoBehaviour
{
    public Slot[] slots;

    [Header("Display")]
    [SerializeField] GameObject storagePanel;
    [SerializeField] TextMeshProUGUI itemName;
    [SerializeField] Sprite tempIcon;

    [Header("Setup")]
    [SerializeField] GameObject slotPrefab;
    [SerializeField] Transform slotHolder, hotbarHolder;
    [SerializeField] int slotCount = 20;
    public ButtonGroup storageGroup, hotbarGroup;

    [Header("Drag And Drop")]
    [SerializeField] Image dragIcon;
    [SerializeField] int currentSlotID = -1;
    [SerializeField] int hoveredSlotID = -1;

    Inventory inventory;

    // save last selected hotbar slot
    // upon closing the storage, if current selected slot is not in hotbar, 
    // revert to last selected hotbar slot
    int lastHotbarID = 0;
    int selectedSlotID = 0;

    void Start()
    {
        GenerateSlots();
        inventory = GameManager.instance.inventory;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            // Refresh(inventory.myInventory);

            if (storagePanel != null)
                if (!storagePanel.activeSelf)
                    OpenStorage();
                else
                    CloseStorage();
        }
    }

    public void OpenStorage()
    {
        storagePanel.SetActive(true);
    }

    public void CloseStorage()
    {
        storagePanel.SetActive(false);
        if (selectedSlotID >= 10 && slots[lastHotbarID] != null)
            slots[lastHotbarID].button.onClick.Invoke();
    }

    void GenerateSlots()
    {
        // Only return if the slots array is fully initialized and all slots are generated/assigned
        if (slots != null && slots.Length == slotCount && slots[slotCount - 1] != null)
            return;

        System.Array.Resize(ref slots, slotCount);

        // Retrieve and initialize the first 10 hotbar slots from hotbarHolder
        if (hotbarHolder != null)
        {
            int existingCount = Mathf.Min(10, hotbarHolder.childCount);
            for (int i = 0; i < existingCount; i++)
            {
                if (slots[i] == null)
                    slots[i] = hotbarHolder.GetChild(i).GetComponent<Slot>();

                if (slots[i] != null)
                {
                    slots[i].Initialize(i, this, hotbarGroup);

                    slots[i].ClearItem();
                    slots[i].SetQuantity(0);
                    slots[i].SetIcon(null);

                    if (hotbarGroup != null && !hotbarGroup.buttons.Contains(slots[i].button.image))
                        hotbarGroup.buttons.Add(slots[i].button.image);
                }
            }
        }
        else
        {
            Debug.LogWarning("InventoryDisplay: hotbarHolder is not assigned.");
        }

        // Generate new slots starting from ID = 10 (storage)
        for (int i = 10; i < slotCount; i++)
        {
            if (slots[i] == null)
            {
                GameObject newSlot = Instantiate(slotPrefab, slotHolder);
                slots[i] = newSlot.GetComponent<Slot>();
            }

            if (slots[i] != null)
            {
                slots[i].Initialize(i, this, storageGroup);

                slots[i].ClearItem();
                slots[i].SetQuantity(0);
                slots[i].SetIcon(null);

                if (storageGroup != null && !storageGroup.buttons.Contains(slots[i].button.image))
                    storageGroup.buttons.Add(slots[i].button.image);
            }
        }
    }

    public void Refresh(Dictionary<string, InventoryEntry> myInventory)
    {
        if (slots == null || slots.Length == 0)
            GenerateSlots();

        // 1. Clean up slots: if the item in slot is not in myInventory or has quantity <= 0, reset that slot.
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            string itemName = slots[i].itemName;
            if (!string.IsNullOrEmpty(itemName))
            {
                if (myInventory == null || !myInventory.TryGetValue(itemName, out InventoryEntry e) || e.quantity <= 0)
                    slots[i].ClearItem();
            }
        }

        // 2. Map new active items to empty slots
        if (myInventory != null)
        {
            foreach (var entry in myInventory)
            {
                if (entry.Value.quantity > 0)
                {
                    // Check if already assigned to a slot
                    bool alreadyAssigned = false;
                    for (int i = 0; i < slots.Length; i++)
                    {
                        if (slots[i] == null) continue;
                        if (slots[i].itemName == entry.Key)
                        {
                            alreadyAssigned = true;
                            break;
                        }
                    }

                    if (!alreadyAssigned)
                    {
                        // Find first empty slot
                        int emptyIndex = -1;
                        for (int i = 0; i < slots.Length; i++)
                        {
                            if (slots[i] == null) continue;
                            if (string.IsNullOrEmpty(slots[i].itemName))
                            {
                                emptyIndex = i;
                                break;
                            }
                        }

                        if (emptyIndex >= 0)
                            slots[emptyIndex].SetItem(entry.Key, inventory.GetItemType(entry.Key));
                        else
                            Debug.LogWarning($"Inventory is full, cannot display: {entry.Key}");
                    }
                }
            }
        }

        // 3. Update the slots in UI
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;

            string itemName = slots[i].itemName;
            if (!string.IsNullOrEmpty(itemName) && myInventory != null && myInventory.TryGetValue(itemName, out InventoryEntry entry) && entry.quantity > 0)
            {
                slots[i].SetQuantity(entry.quantity);

                Sprite icon = Resources.Load<Sprite>("Icons/" + itemName);
                if (icon == null)
                {
                    Debug.LogWarning($"Resource icon not found: Icons/{itemName}");
                    icon = tempIcon;
                }
                slots[i].SetIcon(icon);
            }
            else
            {
                slots[i].SetQuantity(0);
                slots[i].SetIcon(null);
            }
        }
    }

    public void RefreshSlot(int ID, Dictionary<string, InventoryEntry> myInventory)
    {
        if (slots == null || ID < 0 || ID >= slots.Length || slots[ID] == null) return;

        string itemName = slots[ID].itemName;
        if (!string.IsNullOrEmpty(itemName) && myInventory != null && myInventory.TryGetValue(itemName, out InventoryEntry entry) && entry.quantity > 0)
        {
            slots[ID].SetQuantity(entry.quantity);

            Sprite icon = Resources.Load<Sprite>("Icons/" + itemName);
            if (icon == null)
            {
                Debug.LogWarning($"Resource icon not found: Icons/{itemName}");
                icon = tempIcon;
            }
            slots[ID].SetIcon(icon);
        }
        else
        {
            slots[ID].SetQuantity(0);
            slots[ID].SetIcon(null);
        }
    }

    public void SelectSlot(int ID)
    {
        if (slots == null || ID < 0 || ID >= slots.Length || slots[ID] == null) return;

        selectedSlotID = ID;
        if (ID < 10)
        {
            lastHotbarID = ID;
            if (!string.IsNullOrEmpty(slots[ID].itemName))
            {
                itemName.text = slots[ID].itemName;
                GameManager.instance.pm.ChangeTool(slots[ID].type, slots[ID].itemName);
            }
            else
                GameManager.instance.pm.ChangeTool(ItemType.none, "");
        }
        else
        {
            if (!string.IsNullOrEmpty(slots[ID].itemName))
                itemName.text = slots[ID].itemName;
            else
                itemName.text = "";
        }
    }

    // Drag and Drop

    public void BeginDrag(int slotID, PointerEventData eventData)
    {
        if (slots == null || slotID < 0 || slotID >= slots.Length || slots[slotID] == null) return;
        if (slots[slotID].iconImage.sprite == null) return;

        currentSlotID = slotID;
        hoveredSlotID = slotID;

        dragIcon.sprite = slots[slotID].iconImage.sprite;
        dragIcon.enabled = dragIcon.sprite != null;

        UpdateDrag(eventData);
    }

    public void UpdateDrag(PointerEventData eventData)
    {
        // handled using MouseFollow.cs
    }

    public void EndDrag(int slotID)
    {
        // Hide the drag icon
        dragIcon.sprite = null;
        dragIcon.enabled = false;

        if (currentSlotID >= 0 && hoveredSlotID >= 0 && hoveredSlotID != currentSlotID)
        {
            SwapSlots(currentSlotID, hoveredSlotID);
            SelectSlot(hoveredSlotID);
            if (slots[hoveredSlotID] != null)
                slots[hoveredSlotID].button.onClick.Invoke();
        }
        
        currentSlotID = -1;
        hoveredSlotID = -1;
    }

    public void SetHoveredSlot(int slotID)
    {
        hoveredSlotID = slotID;
    }

    public void ClearHoveredSlot(int slotID)
    {
        if (hoveredSlotID == slotID)
            hoveredSlotID = -1;
    }

    private void SwapSlots(int a, int b)
    {
        if (slots == null || a < 0 || b < 0 || a >= slots.Length || b >= slots.Length) return;
        if (slots[a] == null || slots[b] == null) return;

        (string nameA, ItemType typeA) = (slots[a].itemName, slots[a].type);
        slots[a].SetItem(slots[b].itemName, slots[b].type);
        slots[b].SetItem(nameA, typeA);

        Sprite iconA = slots[a].iconImage.sprite;
        int quantityA = slots[a].n;

        slots[a].SetIcon(slots[b].iconImage.sprite);
        slots[a].SetQuantity(slots[b].n);

        slots[b].SetIcon(iconA);
        slots[b].SetQuantity(quantityA);
    }
}
