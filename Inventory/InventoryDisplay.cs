using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventoryDisplay : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] GameObject storagePanel;
    [SerializeField] TextMeshProUGUI itemName;
    [SerializeField] Sprite tempIcon;

    [Header("Setup")]
    [SerializeField] GameObject slotPrefab;
    [SerializeField] Transform slotHolder;
    [SerializeField] int slotCount = 20;
    public ButtonGroup myGroup;

    [Header("Drag And Drop")]
    [SerializeField] Image dragIcon;
    [SerializeField] int currentSlotID = -1;
    [SerializeField] int hoveredSlotID = -1;

    Inventory inventory;
    Slot[] slots;
    string[] slotItems;
    
    void Start()
    {
        GenerateSlots();
        inventory = GameManager.instance.inventory;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Refresh(inventory.myInventory);

            if (storagePanel != null)
                storagePanel.SetActive(!storagePanel.activeSelf);
        }
    }

    void GenerateSlots()
    {
        if (slots != null && slots.Length > 0)
            return;

        slots = new Slot[slotCount];
        slotItems = new string[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, slotHolder);
            Slot slot = newSlot.GetComponent<Slot>();

            if (slot == null)
                continue;

            slot.Initialize(i, this);
            slots[i] = slot;

            slotItems[i] = string.Empty;
            slot.SetQuantity(0);
            slot.SetIcon(null);
            
            myGroup.buttons.Add(slot.button.image);
        }
    }

    public void Refresh(Dictionary<string, int> myInventory)
    {
        if (slots == null || slots.Length == 0)
            GenerateSlots();

        if (slotItems == null || slotItems.Length != slots.Length)
            slotItems = new string[slots.Length];

        // 1. Clean up slotItems: if the item in slotItems[i] is not in myInventory or has quantity <= 0, reset that slot.
        for (int i = 0; i < slotItems.Length; i++)
        {
            string itemName = slotItems[i];
            if (!string.IsNullOrEmpty(itemName))
            {
                if (myInventory == null || !myInventory.TryGetValue(itemName, out int n) || n <= 0)
                    slotItems[i] = string.Empty;
            }
        }

        // 2. Map new active items to empty slots
        if (myInventory != null)
        {
            foreach (var entry in myInventory)
            {
                if (entry.Value > 0)
                {
                    // Check if already assigned to a slot
                    bool alreadyAssigned = false;
                    for (int i = 0; i < slotItems.Length; i++)
                    {
                        if (slotItems[i] == entry.Key)
                        {
                            alreadyAssigned = true;
                            break;
                        }
                    }

                    if (!alreadyAssigned)
                    {
                        // Find first empty slot
                        int emptyIndex = -1;
                        for (int i = 0; i < slotItems.Length; i++)
                        {
                            if (string.IsNullOrEmpty(slotItems[i]))
                            {
                                emptyIndex = i;
                                break;
                            }
                        }

                        if (emptyIndex >= 0)
                            slotItems[emptyIndex] = entry.Key;
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

            string itemName = slotItems[i];
            if (!string.IsNullOrEmpty(itemName) && myInventory != null && myInventory.TryGetValue(itemName, out int N) && N > 0)
            {
                slots[i].SetQuantity(N);

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

    public void RefreshSlot(int ID, Dictionary<string, int> myInventory)
    {
        if (slots[ID] == null) return;

        string itemName = slotItems[ID];
        if (!string.IsNullOrEmpty(itemName) && myInventory != null && myInventory.TryGetValue(itemName, out int N) && N > 0)
        {
            slots[ID].SetQuantity(N);

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
        if (!string.IsNullOrEmpty(slotItems[ID]))
            itemName.text = slotItems[ID];
    }

    // Drag and Drop

    public void BeginDrag(int slotID, PointerEventData eventData)
    {
        if (slotID < 0 || slotID >= slots.Length) return;
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
        if (slotItems == null || a < 0 || b < 0 || a >= slotItems.Length || b >= slotItems.Length) return;

        (slotItems[a], slotItems[b]) = (slotItems[b], slotItems[a]);
        
        Sprite iconA = slots[a].iconImage.sprite;
        int quantityA = slots[a].n;

        slots[a].SetIcon(slots[b].iconImage.sprite);
        slots[a].SetQuantity(slots[b].n);

        slots[b].SetIcon(iconA);
        slots[b].SetQuantity(quantityA);
    }
}
