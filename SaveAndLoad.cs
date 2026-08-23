using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class GardenSaveData
{
    public InventorySaveData inventory;
    public List<GrowableSaveData> growables;
}

[System.Serializable]
public class InventorySaveData
{
    public float exp;
    public int level;
    public int coin;
    public List<InventoryItemSaveData> inventoryList;
}

[System.Serializable]
public class InventoryItemSaveData
{
    public string name;
    public int quantity;
    public ItemType type;
}

[System.Serializable]
public class GrowableSaveData
{
    public int plantID;
    public List<int> slotIDs;
    public Vector3 position;
    public Quaternion rotation;
    public float growthIndex;
    public float maxGrowth;
    public float wiggleOffset;
    public float wiggleAmplitude;
}

public class SaveAndLoad : MonoBehaviour
{
    GameManager gm;

    void Start()
    {
        gm = GameManager.instance;
    }

    public void SaveGarden(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("Save path is null or empty!");
            return;
        }

        if (gm == null) gm = GameManager.instance;
        if (gm == null || gm.inventory == null)
        {
            Debug.LogError("GameManager or Inventory is null, cannot save garden!");
            return;
        }

        GardenSaveData saveData = new GardenSaveData();

        // 1. Save inventory info
        Inventory inv = gm.inventory;
        saveData.inventory = new InventorySaveData
        {
            exp = inv.exp,
            level = inv.level,
            coin = inv.coin,
            inventoryList = new List<InventoryItemSaveData>()
        };

        if (inv.myInventory != null)
        {
            foreach (var entry in inv.myInventory)
            {
                saveData.inventory.inventoryList.Add(new InventoryItemSaveData
                {
                    name = entry.Key,
                    quantity = entry.Value.quantity,
                    type = entry.Value.type
                });
            }
        }

        // 2. Save Growables info
        saveData.growables = new List<GrowableSaveData>();
        Growable[] allGrowables = FindObjectsOfType<Growable>();
        foreach (Growable g in allGrowables)
        {
            // Ignore chopped trees and products
            if (g.isProduct || g.chopped)
                continue;

            // Get slot IDs that have children (products)
            List<int> slotIDs = new List<int>();
            if (g.slots != null)
            {
                for (int i = 0; i < g.slots.Length; i++)
                {
                    if (g.slots[i] != null && g.slots[i].childCount > 0)
                    {
                        slotIDs.Add(i);
                    }
                }
            }

            Transform target = g.transform.parent;
            Vector3 position = target.position;
            Quaternion rotation = Quaternion.Euler(0f, target.eulerAngles.y, 0f);

            saveData.growables.Add(new GrowableSaveData
            {
                plantID = g.productID,
                slotIDs = slotIDs,
                position = position,
                rotation = rotation,
                growthIndex = g.growthIndex,
                maxGrowth = g.maxGrowth,
                wiggleOffset = g.wiggleOffset,
                wiggleAmplitude = g.wiggleAmplitude
            });
        }

        // 3. Serialize and write to file
        try
        {
            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(path, json);
            Debug.Log($"Garden saved successfully to {path}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save garden to {path}: {ex.Message}");
        }
    }

    public void LoadGarden(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            Debug.LogError($"Save path '{path}' does not exist!");
            return;
        }

        if (gm == null) gm = GameManager.instance;
        if (gm == null || gm.inventory == null)
        {
            Debug.LogError("GameManager or Inventory is null, cannot load garden!");
            return;
        }

        string json = File.ReadAllText(path);
        GardenSaveData saveData = JsonUtility.FromJson<GardenSaveData>(json);
        if (saveData == null)
        {
            Debug.LogError("Failed to deserialize garden save data!");
            return;
        }

        // 1. Restore Inventory
        Inventory inv = gm.inventory;
        inv.level = saveData.inventory.level;
        inv.expToNextLvl = 100f + saveData.inventory.level * 10f;
        inv.exp = saveData.inventory.exp;
        inv.coin = saveData.inventory.coin;

        if (saveData.inventory.inventoryList != null)
        {
            foreach (var savedItem in saveData.inventory.inventoryList)
            {
                inv.myInventory[savedItem.name] = new InventoryEntry(savedItem.quantity, savedItem.type);
            }
        }
        inv.myDisplay.Refresh(inv.myInventory);
        inv.shop.RefreshShop();
        inv.selection.RefreshPlants();

        // 2. Clear existing active growables by deleting their parent objects (or themselves if no parent)
        Growable[] activeGrowables = FindObjectsOfType<Growable>();
        foreach (var g in activeGrowables)
        {
            if (g.transform.parent != null)
            {
                g.transform.parent.gameObject.SetActive(false);
                Destroy(g.transform.parent.gameObject);
            }
            else
            {
                g.gameObject.SetActive(false);
                Destroy(g.gameObject);
            }
        }

        // 3. Respawn saved growables
        PlantTool plantTool = FindObjectOfType<PlantTool>();
        if (plantTool == null)
        {
            Debug.LogError("PlantTool not found in scene, cannot spawn plants!");
            return;
        }

        if (saveData.growables != null)
        {
            foreach (var savedTree in saveData.growables)
            {
                // Find parent if it's an oven tree
                Transform ovenParent = null;
                if (savedTree.plantID >= 0 && savedTree.plantID < inv.foodList.Count)
                {
                    if (inv.foodList[savedTree.plantID].type == "Oven")
                    {
                        Collider[] colliders = Physics.OverlapSphere(savedTree.position, 0.5f);
                        foreach (var col in colliders)
                        {
                            if (col.CompareTag("Oven"))
                            {
                                ovenParent = col.transform.parent;
                                break;
                            }
                        }
                    }
                }

                // Instantiate plant using PlantTool
                Growable g = plantTool.Plant(savedTree.plantID, savedTree.position, savedTree.rotation, ovenParent);
                if (g != null)
                {
                    // Restore stats
                    if (savedTree.maxGrowth > 0.001f) g.maxGrowth = savedTree.maxGrowth;
                    g.growthIndex = savedTree.growthIndex;
                    g.transform.localScale = Vector3.one * g.growthIndex;
                    g.wiggleOffset = savedTree.wiggleOffset;
                    g.wiggleAmplitude = savedTree.wiggleAmplitude;

                    // Respawn products in slots
                    if (savedTree.slotIDs != null)
                    {
                        foreach (int slotID in savedTree.slotIDs)
                        {
                            Growable newFruit = g.GrowFruitAtSlot(slotID);
                            if (newFruit != null)
                            {
                                newFruit.growthIndex = 0.3f * newFruit.maxGrowth;
                                newFruit.transform.localScale = Vector3.one * newFruit.growthIndex;
                            }
                        }
                    }
                }
            }
        }

        Debug.Log($"Garden loaded successfully from {path}");
    }

#if UNITY_EDITOR
    [ContextMenu("Save Garden As...")]
    public void SaveGardenDialog()
    {
        string path = UnityEditor.EditorUtility.SaveFilePanel("Save Garden State", "", "garden_save.json", "json");
        if (!string.IsNullOrEmpty(path))
        {
            SaveGarden(path);
        }
    }

    [ContextMenu("Load Garden From...")]
    public void LoadGardenDialog()
    {
        string path = UnityEditor.EditorUtility.OpenFilePanel("Load Garden State", "", "json");
        if (!string.IsNullOrEmpty(path))
        {
            LoadGarden(path);
        }
    }
#endif
}
