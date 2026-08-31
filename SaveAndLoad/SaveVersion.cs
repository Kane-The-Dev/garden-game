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
    SaveAndLoad manager;

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

        manager.SaveGarden(filePath);

        string json = File.ReadAllText(filePath);
        GardenSaveData refreshedData = JsonUtility.FromJson<GardenSaveData>(json);
        if (refreshedData != null)
            Refresh(Path.GetFileNameWithoutExtension(filePath), refreshedData);
    }

    public void LoadThisVersion()
    {
        if (manager == null || string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("SaveVersion: not initialized with a manager/path!");
            return;
        }

        manager.LoadGarden(filePath);
    }

    public void DeleteThisVersion()
    {
        if (manager == null || string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("SaveVersion: not initialized with a manager/path!");
            return;
        }

        if (manager.DeleteSave(filePath))
            Destroy(gameObject);
    }

    public void RenameThisVersion(string newName)
    {
        if (manager == null || string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("SaveVersion: not initialized with a manager/path!");
            return;
        }

        if (string.IsNullOrWhiteSpace(newName))
        {
            Debug.LogWarning("Save name can't be empty.");
            return;
        }

        string dir = Path.GetDirectoryName(filePath);
        string oldPath = filePath;

        if (manager.RenameSave(oldPath, newName))
        {
            filePath = Path.Combine(dir, newName + ".json");
            gardenName.text = newName;
        }
    }
}