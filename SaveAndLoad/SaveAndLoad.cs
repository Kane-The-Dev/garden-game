using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class GardenSaveData
{
    public string version;
    public string savedAt;
    public int daysPassed;
    public float ingameTime;
    public int fenceLevel;
    public InventorySaveData inventory;
    public List<GrowableSaveData> growables;
    public List<ConstructibleSaveData> constructibles;
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
    public int slotID;
}

[System.Serializable]
public class ConstructibleSaveData
{
    public int buildID;
    public Vector3 position;
    public Quaternion rotation;
}

[System.Serializable]
public class GrowableSaveData
{
    public int plantID;
    public int treeID;
    public List<float> slotGrowthIndices; // 0 means empty slot
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
    [SerializeField] List<ToolUnlock> toolUnlocks = new();
    public string saveFolder = "Saves";
    public string SaveFolderPath => Path.IsPathRooted(saveFolder) ?
        saveFolder : Path.Combine(Application.persistentDataPath, saveFolder);

    [Header("Autosave settings")]
    public string autosavePrefix = "autosave_";
    public bool autosaveEnabled = true;
    public float autosaveInterval = 300f;
    public int maxAutosaves = 10;
    float autosaveTimer;
    
    void Awake()
    {
        saveFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "../Saves"));
    }

    void Start()
    {
        gm = GameManager.instance;
        Directory.CreateDirectory(SaveFolderPath); // make sure the folder exists on first run
    }

    void Update()
    {
        if (!autosaveEnabled) return;
        autosaveTimer += Time.deltaTime;
        if (autosaveTimer >= autosaveInterval)
        {
            autosaveTimer = 0f;
            Autosave();
        }
    }

    // SAVE
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

        GardenSaveData saveData = new GardenSaveData
        {
            version = gm.version,
            savedAt = System.DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"),
            daysPassed = gm.clock.dayCount,
            ingameTime = gm.clock.time,
            fenceLevel = gm.fence.myLevel
        };

        // 1. Save inventory info
        Inventory inv = gm.inventory;
        saveData.inventory = new InventorySaveData
        {
            exp = inv.exp,
            level = inv.level,
            coin = inv.coin,
            inventoryList = new List<InventoryItemSaveData>()
        };

        foreach (var kvp in inv.myInventory)
        {
            if (kvp.Value.quantity <= 0) continue;

            saveData.inventory.inventoryList.Add(new InventoryItemSaveData
            {
                name = kvp.Key,
                quantity = kvp.Value.quantity,
                type = kvp.Value.type,
                slotID = kvp.Value.slotID // value = -1 if the item never got a slot (inventory was full)
            });
        }

        // 2. Save Constructibles info
        saveData.constructibles = new List<ConstructibleSaveData>();
        Constructible[] allConstructibles = FindObjectsOfType<Constructible>();
        foreach (Constructible c in allConstructibles)
        {
            // Ignore previews and chopped buildings
            if (c.isPreview || c.chopped)
                continue;

            Transform target = c.transform.parent != null ? c.transform.parent : c.transform;
            Vector3 position = target.position;
            Quaternion rotation = Quaternion.Euler(0f, target.eulerAngles.y, 0f);

            int buildID = c.buildID;
            if (buildID < 0) continue;

            saveData.constructibles.Add(new ConstructibleSaveData
            {
                buildID = buildID,
                position = position,
                rotation = rotation
            });
        }

        // 3. Save Growables info
        saveData.growables = new List<GrowableSaveData>();
        Growable[] allGrowables = FindObjectsOfType<Growable>();
        foreach (Growable g in allGrowables)
        {
            // Ignore chopped trees and products
            if (g.isProduct || g.chopped)
                continue;

            // Save per-slot fruit growthIndex; 0 means the slot is empty.
            List<float> slotGrowthIndices = new List<float>();
            if (g.slots != null)
            {
                for (int i = 0; i < g.slots.Length; i++)
                {
                    if (g.slots[i] != null && g.slots[i].childCount > 0)
                    {
                        Growable fruit = g.slots[i].GetChild(0).GetComponent<Growable>();
                        slotGrowthIndices.Add(fruit != null ? fruit.growthIndex : 0f);
                    }
                    else
                        slotGrowthIndices.Add(0f);
                }
            }

            Transform target = g.transform.parent;
            Vector3 position = target.position;
            Quaternion rotation = Quaternion.Euler(0f, target.eulerAngles.y, 0f);

            saveData.growables.Add(new GrowableSaveData
            {
                plantID = g.productID,
                treeID = g.treeID,
                slotGrowthIndices = slotGrowthIndices,
                position = position,
                rotation = rotation,
                growthIndex = g.growthIndex,
                maxGrowth = g.maxGrowth,
                wiggleOffset = g.wiggleOffset,
                wiggleAmplitude = g.wiggleAmplitude
            });
        }

        // 4. Serialize and write to file (atomic: write to temp, then swap in)
        try
        {
            string json = JsonUtility.ToJson(saveData, true);
            string tempPath = path + ".tmp";

            File.WriteAllText(tempPath, json);

            if (File.Exists(path))
                File.Replace(tempPath, path, null);
            else
                File.Move(tempPath, path);

            Debug.Log($"Garden saved successfully to {path}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save garden to {path}: {ex.Message}");
        }
    }

    IEnumerator MuteTemporarily(float duration)
    {
        Settings settings = FindObjectOfType<Settings>();
        if (settings == null) yield break;

        float originalSFX = settings.SFXVolume;
        settings.SFXVolume = 0f;
        settings.ApplyVolumes();

        yield return new WaitForSecondsRealtime(duration);

        if (settings != null)
        {
            settings.SFXVolume = originalSFX;
            settings.ApplyVolumes();
        }
    }

    // LOAD
    public void LoadGarden(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            Debug.LogError($"Save path '{path}' does not exist!");
            return;
        }

        StartCoroutine(MuteTemporarily(2f));

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

        gm.clock.dayCount = saveData.daysPassed;
        gm.clock.time = saveData.ingameTime;
        gm.fence.SetFence(saveData.fenceLevel);

        // 1. Restore Inventory
        Inventory inv = gm.inventory;
        inv.level = saveData.inventory.level;
        inv.expToNextLvl = 100f + saveData.inventory.level * 10f;
        inv.exp = saveData.inventory.exp;
        inv.coin = saveData.inventory.coin;

        // Clear current inventory dict and display before loading saved data
        inv.myInventory.Clear();
        inv.myDisplay.ClearAllSlots();

        if (saveData.inventory.inventoryList != null)
        {
            foreach (var savedItem in saveData.inventory.inventoryList)
            {
                int slotID = savedItem.slotID;

                // if item wasn't assigned to any slot when saved -> find a new slot during load
                if (slotID < 0)
                    slotID = inv.myDisplay.FindEmptySlot();

                InventoryEntry entry = new InventoryEntry(savedItem.quantity, savedItem.type, slotID);
                inv.myInventory[savedItem.name] = entry;

                if (slotID >= 0)
                {
                    inv.myDisplay.PlaceItemAtSlot(slotID, savedItem.name, savedItem.type);
                    inv.myDisplay.RefreshSlot(slotID, savedItem.name, entry);
                }
                else
                    Debug.LogWarning($"No available slot to display loaded item: {savedItem.name}");
            }
        }

        // Restore purchased tools (basic tools are handled by PlantManager)
        foreach (var savedItem in saveData.inventory.inventoryList)
        {
            if (savedItem.type != ItemType.water &&
                savedItem.type != ItemType.harvest &&
                savedItem.type != ItemType.chop) continue;

            ToolUnlock match = toolUnlocks.Find(t => t.itemName == savedItem.name);
            if (match != null)
            {
                match.RestoreTool(savedItem.quantity);
                inv.shop.stock[match] = 0;
            }
            else
                Debug.LogWarning($"SaveAndLoad: no ToolUnlock asset found for saved tool '{savedItem.name}'");
        }

        inv.shop.RefreshShop();
        inv.selection.RefreshPlants();
        inv.fs.UpdateStorage();

        // 2. Clear existing active growables and constructibles
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

        Constructible[] activeConstructibles = FindObjectsOfType<Constructible>();
        foreach (var c in activeConstructibles)
        {
            if (c.isPreview) continue;
            GameObject root = c.transform.parent != null ? c.transform.parent.gameObject : c.gameObject;
            root.SetActive(false);
            Destroy(root);
        }

        // 3. Respawn saved constructibles
        BuildTool buildTool = FindObjectOfType<BuildTool>();
        if (buildTool == null)
        {
            Debug.LogError("BuildTool not found in scene, cannot spawn buildings!");
        }
        else if (saveData.constructibles != null)
        {
            foreach (var savedBuilding in saveData.constructibles)
            {
                buildTool.Build(savedBuilding.buildID, savedBuilding.position, savedBuilding.rotation);
            }
        }

        Debug.Log($"Garden loaded successfully from {path}");

        // 4. Respawn saved growables
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
                Growable g = plantTool.Plant(
                    savedTree.plantID,
                    savedTree.position,
                    savedTree.rotation,
                    ovenParent,
                    savedTree.treeID
                );
                if (g != null)
                {
                    // Restore stats
                    if (savedTree.maxGrowth > 0.001f) g.maxGrowth = savedTree.maxGrowth;
                    g.growthIndex = savedTree.growthIndex;
                    g.transform.localScale = Vector3.one * g.growthIndex;
                    g.wiggleOffset = savedTree.wiggleOffset;
                    g.wiggleAmplitude = savedTree.wiggleAmplitude;

                    // Respawn products in slots, restoring growthIndex per fruit
                    if (savedTree.slotGrowthIndices != null)
                    {
                        for (int i = 0; i < savedTree.slotGrowthIndices.Count; i++)
                        {
                            float savedGrowth = savedTree.slotGrowthIndices[i];
                            if (savedGrowth <= 0f) continue; // empty slot

                            Growable newFruit = g.GrowFruitAtSlot(i);
                            if (newFruit != null)
                            {
                                newFruit.growthIndex = savedGrowth;
                                newFruit.transform.localScale = Vector3.one * savedGrowth;
                            }
                        }
                    }
                }
            }
        }
    }

    // READ, RENAME, DELETE
    public Dictionary<string, GardenSaveData> GetAllSaves()
    {
        var result = new Dictionary<string, GardenSaveData>();

        if (!Directory.Exists(SaveFolderPath))
            return result;

        foreach (string path in Directory.GetFiles(SaveFolderPath, "*.json"))
        {
            try
            {
                string json = File.ReadAllText(path);
                GardenSaveData data = JsonUtility.FromJson<GardenSaveData>(json);
                if (data != null)
                    result[path] = data;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Skipping corrupt save file {path}: {ex.Message}");
            }
        }

        return result;
    }

    public bool DeleteSave(string path)
    {
        try
        {
            File.Delete(path);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to delete {path}: {ex.Message}");
            return false;
        }
    }

    public bool RenameSave(string oldPath, string newName)
    {
        string dir = Path.GetDirectoryName(oldPath);
        string newPath = Path.Combine(dir, newName + ".json");

        if (File.Exists(newPath))
        {
            Debug.LogWarning($"A save named '{newName}' already exists.");
            return false;
        }

        try
        {
            File.Move(oldPath, newPath);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to rename {oldPath}: {ex.Message}");
            return false;
        }
    }

    // AUTOSAVE
    public void Autosave()
    {
        var autosaves = Directory.GetFiles(SaveFolderPath, autosavePrefix + "*.json")
            .OrderBy(File.GetLastWriteTime).ToList();

        string path = autosaves.Count >= maxAutosaves
            ? autosaves[0] // reuse the oldest slot
            : Path.Combine(SaveFolderPath, autosavePrefix + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json");

        SaveGarden(path);
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