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
    // separate item name displays for board and mouse follow
    [SerializeField] TextMeshProUGUI itemName_Board, itemName_Mouse, itemDescription; 
    [SerializeField] Animator itemNameAnimator;
    [SerializeField] Sprite tempIcon;

    [Header("Setup")]
    [SerializeField] GameObject slotPrefab;
    [SerializeField] Transform slotHolder, hotbarHolder;
    [SerializeField] int slotCount = 20, hotbarCount = 6, maxHotbarCount = 10;
    public ButtonGroup storageGroup, hotbarGroup;

    [Header("Drag And Drop")]
    [SerializeField] Image dragIcon;
    [SerializeField] int currentSlotID = -1;
    [SerializeField] int hoveredSlotID = -1;

    Inventory inventory;
    GameManager gm;

    // save last selected hotbar slot
    // upon closing the storage, if current selected slot is not in hotbar, 
    // revert to last selected hotbar slot
    int lastHotbarID = 0;
    int selectedSlotID = 0;

    void Start()
    {
        GenerateSlots();
        gm = GameManager.instance;
        inventory = gm.inventory;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            // Refresh(inventory.myInventory);

            if (storagePanel != null)
                if (!storagePanel.activeSelf)
                    OpenBackpack();
                else
                    CloseBackpack();
        }
    }

    public void OpenBackpack()
    {
        gm.UIAnimator.SetTrigger("openbag");
        gm.cam.movable = false;
    }

    public void CloseBackpack()
    {
        gm.UIAnimator.SetTrigger("closebag");
        gm.cam.movable = true;
        
        if (selectedSlotID >= maxHotbarCount && slots[lastHotbarID] != null)
            slots[lastHotbarID].button.onClick.Invoke();
    }

    public void UnlockHotbarSlot()
    {
        if (hotbarCount >= maxHotbarCount) return;
        hotbarHolder.GetChild(hotbarCount++).gameObject.SetActive(true);
    }

    // Slot management

    void GenerateSlots()
    {
        // Only return if the slots array is fully initialized and all slots are generated
        if (slots != null && slots.Length == slotCount && slots[slotCount - 1] != null)
            return;
        
        System.Array.Resize(ref slots, slotCount);

        // Retrieve and initialize ALL hotbar slots from hotbarHolder
        if (hotbarHolder != null)
        {
            int existingCount = Mathf.Min(maxHotbarCount, hotbarHolder.childCount);
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

                    if (hotbarGroup != null && slots[i].button != null && slots[i].button.image != null && !hotbarGroup.buttons.Contains(slots[i].button.image))
                        hotbarGroup.buttons.Add(slots[i].button.image);
                }
            }
        }
        else
            Debug.LogWarning("InventoryDisplay: hotbarHolder is not assigned.");

        // Generate and initialize storage slots
        for (int i = maxHotbarCount; i < slotCount; i++)
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

                if (storageGroup != null && slots[i].button != null && slots[i].button.image != null && !storageGroup.buttons.Contains(slots[i].button.image))
                    storageGroup.buttons.Add(slots[i].button.image);
            }
        }
    }

    public void ClearAllSlots()
    {
        if (slots == null) return;
        foreach (var slot in slots)
        {
            if (slot == null) continue;
            slot.ClearItem();
            slot.SetQuantity(0);
            slot.SetIcon(null);
        }
    }

    public int FindEmptySlot()
    {
        if (slots == null || slots.Length == 0)
            GenerateSlots();

        if (slots == null) return -1;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            if (i >= hotbarCount && i < maxHotbarCount) continue;
            if (string.IsNullOrEmpty(slots[i].itemName))
                return i;
        }
        return -1;
    }

    public void PlaceItemAtSlot(int slotID, string itemName, ItemType type)
    {
        if (slots == null || slotID < 0 || slotID >= slots.Length) return;
        if (slots[slotID] == null) return;

        slots[slotID].SetItem(itemName, type);

        // Keep the inventory entry's slotID value in sync
        if (inventory != null && inventory.myInventory.TryGetValue(itemName, out InventoryEntry e))
            inventory.myInventory[itemName] = new InventoryEntry(e.quantity, e.type, slotID);
    }

    public void RefreshSlot(int slotID, string itemName, InventoryEntry entry)
    {
        if (slots == null || slotID < 0 || slotID >= slots.Length || slots[slotID] == null) return;

        if (entry.quantity > 0)
        {
            slots[slotID].SetItem(itemName, entry.type);
            slots[slotID].SetQuantity(entry.quantity);

            Sprite icon = ReadFile.LoadIconSprite(itemName);
            if (icon == null) icon = tempIcon;
            slots[slotID].SetIcon(icon);
        }
        else
        {
            // quantity dropped to 0 -> free the slot
            slots[slotID].ClearItem();
            slots[slotID].SetQuantity(0);
            slots[slotID].SetIcon(null);
            gm.pm.ChangeTool(ItemType.none, "");
        }
    }

    public void SelectSlot(int ID)
    {
        if (slots == null || ID < 0 || ID >= slots.Length || slots[ID] == null) return;

        selectedSlotID = ID;

        // Update board and assign item to PlantManager
        Slot slot = slots[ID];
        bool hasItem = !string.IsNullOrEmpty(slot.itemName);

        itemName_Board.text = hasItem ? slot.itemName : "";

        if (ID < maxHotbarCount)
        {
            lastHotbarID = ID;
            gm.pm.ChangeTool(hasItem ? slot.type : ItemType.none, hasItem ? slot.itemName : "");
        }

        if (!hasItem) return;

        // Fetch and cache description on first selection
        if (!string.IsNullOrEmpty(slot.description))
        {
            itemDescription.text = slot.description;
            return;
        }

        string lookupName = slot.itemName.Replace('_', ' ');
        Item item = inventory.foodList.Find(x => x.name == lookupName)
                 ?? inventory.buildingList.Find(x => x.name == lookupName)
                 ?? inventory.foodList.Find(x => Inventory.GetProductName(x.name) == lookupName);

        slot.description = item?.description ?? "";
        itemDescription.text = slot.description;
    }

    private void SwapSlots(int a, int b)
    {
        if (slots == null || a < 0 || b < 0 || a >= slots.Length || b >= slots.Length) return;
        if (slots[a] == null || slots[b] == null) return;

        (string nameA, ItemType typeA) = (slots[a].itemName, slots[a].type);
        (string nameB, ItemType typeB) = (slots[b].itemName, slots[b].type);

        slots[a].SetItem(nameB, typeB);
        slots[b].SetItem(nameA, typeA);

        Sprite iconA = slots[a].iconImage.sprite;
        int quantityA = slots[a].n;
        string descriptionA = slots[a].description;

        slots[a].SetIcon(slots[b].iconImage.sprite);
        slots[a].SetQuantity(slots[b].n);
        slots[a].description = slots[b].description;

        slots[b].SetIcon(iconA);
        slots[b].SetQuantity(quantityA);
        slots[b].description = descriptionA;

        // Keep myInventory's slotID values in sync after the swap
        if (inventory != null)
        {
            if (!string.IsNullOrEmpty(nameA) && inventory.myInventory.TryGetValue(nameA, out InventoryEntry eA))
                inventory.myInventory[nameA] = new InventoryEntry(eA.quantity, eA.type, b);

            if (!string.IsNullOrEmpty(nameB) && inventory.myInventory.TryGetValue(nameB, out InventoryEntry eB))
                inventory.myInventory[nameB] = new InventoryEntry(eB.quantity, eB.type, a);
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

    public void SetHoveredSlot(int slotID, string itemName)
    {
        hoveredSlotID = slotID;

        if (!string.IsNullOrEmpty(itemName))
        {
            itemName_Mouse.text = itemName;
            itemNameAnimator.SetBool("hovering", true);
        }
        else
            itemName_Mouse.text = "";
    }

    public void ClearHoveredSlot(int slotID)
    {
        if (hoveredSlotID == slotID)
        {
            hoveredSlotID = -1;
            itemNameAnimator.SetBool("hovering", false);
        }
    }
}
