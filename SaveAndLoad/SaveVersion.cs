using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class SaveVersion : MonoBehaviour
{
    public TextMeshProUGUI gardenName, version, lastSaved, day, level, coin;
    // in-game days passed

    string filePath;
    string currentName => gardenName != null ? gardenName.text : Path.GetFileNameWithoutExtension(filePath);
    SaveAndLoad manager;
    GameManager gm;

    void Start()
    {
        gm = GameManager.instance;
    }

    public void Init(string path, GardenSaveData data, SaveAndLoad saveManager)
    {
        filePath = path;
        manager = saveManager;
        Refresh(Path.GetFileNameWithoutExtension(path), data);
    }

    public void Refresh(string garden, GardenSaveData data)
    {
        gardenName.text = garden;
        version.text = "Version: " + data.version;
        lastSaved.text = "Last saved: " + data.savedAt;
        day.text = data.daysPassed.ToString();
        level.text = data.inventory.level.ToString();
        coin.text = data.inventory.coin.ToString();
    }

    public void SaveThisVersion()
    {
        if (manager == null || string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("SaveVersion: not initialized with a manager/path!");
            return;
        }

        gm.AYSPanel.OpenPanel(
            "Do you want to save garden to '" + currentName + "'?",
            (confirmed, _) =>
            {
                if (confirmed)
                {
                    manager.SaveGarden(filePath);

                    string json = File.ReadAllText(filePath);
                    GardenSaveData refreshedData = JsonUtility.FromJson<GardenSaveData>(json);
                    if (refreshedData != null)
                        Refresh(Path.GetFileNameWithoutExtension(filePath), refreshedData);
                }
            }
        );
    }

    public void LoadThisVersion()
    {
        if (manager == null || string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("SaveVersion: not initialized with a manager/path!");
            return;
        }

        gm.AYSPanel.OpenPanel(
            "Do you want to load garden from '" + currentName + "'?",
            (confirmed, _) =>
            {
                if (confirmed)
                    manager.LoadGarden(filePath);
            }
        );
    }

    public void DeleteThisVersion()
    {
        if (manager == null || string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("SaveVersion: not initialized with a manager/path!");
            return;
        }

        gm.AYSPanel.OpenPanel(
            "Do you want to delete '" + currentName + "'?",
            (confirmed, _) =>
            {
                if (confirmed && manager.DeleteSave(filePath))
                    gm.settings.RefreshSaves();
            }
        );
    }

    public void RenameThisVersion()
    {
        if (manager == null || string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("SaveVersion: not initialized with a manager/path!");
            return;
        }

        TryRename(currentName, "Rename '" + currentName + "' to:\n'\n'");
    }

    void TryRename(string currentName, string message)
    {
        gm.AYSPanel.OpenPanel(
            message,
            (confirmed, newName) =>
            {
                if (!confirmed || string.IsNullOrWhiteSpace(newName))
                    return;

                // Check for duplicate name among existing saves
                var allSaves = manager.GetAllSaves();
                bool nameExists = false;
                foreach (var path in allSaves.Keys)
                {
                    if (Path.GetFileNameWithoutExtension(path).Equals(newName, System.StringComparison.OrdinalIgnoreCase) 
                        && path != filePath)
                    {
                        nameExists = true;
                        break;
                    }
                }

                if (nameExists)
                {
                    TryRename(currentName, "Name already exists!\n'\n'");
                    return;
                }

                string dir = Path.GetDirectoryName(filePath);
                string oldPath = filePath;

                if (manager.RenameSave(oldPath, newName))
                {
                    filePath = Path.Combine(dir, newName + ".json");
                    if (gardenName != null) gardenName.text = newName;
                    gm.settings.RefreshSaves();
                }
            },
            true,
            currentName
        );
    }
}