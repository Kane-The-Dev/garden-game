using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryDisplay : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private GameObject storagePanel;
    [SerializeField] private TextMeshProUGUI itemName;

    [Header("Setup")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotHolder;
    [SerializeField] private int slotCount = 20;

    Inventory inventory;
    private Slot[] slots;
    private string[] slotItems;
    
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
            {
                Debug.LogError($"slotPrefab is missing a Slot component (slot {i}).");
                continue;
            }

            slot.Initialize(i, this);
            slots[i] = slot;

            slotItems[i] = string.Empty;
            slot.SetQuantity(0);
            slot.SetIcon(null);
        }
    }

    public void Refresh(Dictionary<string, int> myInventory)
    {
        if (slots == null || slots.Length == 0)
            GenerateSlots();

        if (slotItems == null || slotItems.Length != slots.Length)
            slotItems = new string[slots.Length];

        // 1. Clean up slotItems: if the item in slotItems[i] is no longer in myInventory or has quantity <= 0, reset that slot.
        for (int i = 0; i < slotItems.Length; i++)
        {
            string itemName = slotItems[i];
            if (!string.IsNullOrEmpty(itemName))
            {
                if (myInventory == null || !myInventory.TryGetValue(itemName, out int qty) || qty <= 0)
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
                    Debug.LogWarning($"Resource icon not found: Icons/{itemName}");

                slots[i].SetIcon(icon);
            }
            else
            {
                slots[i].SetQuantity(0);
                slots[i].SetIcon(null);
            }
        }
    }

    public void SelectSlot(int ID)
    {
        if (!string.IsNullOrEmpty(slotItems[ID]))
            itemName.text = slotItems[ID];
    }
}
