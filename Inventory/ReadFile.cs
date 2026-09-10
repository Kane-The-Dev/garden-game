using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item {

    public int ID;
    public string name;
    public int plantPrice;
    public int sellPrice;
    public float growthSpeed;
    public int levelReq;
    public float weight;
    public string type;
    public string description;

    public void Set(int _ID, string _name, int _plantPrice, 
        int _sellPrice, float _growthSpeed, int _levelReq,
        float _weight, string _type, string _description)
    {
        this.ID = _ID;
        this.name = _name;
        this.plantPrice = _plantPrice;
        this.sellPrice = _sellPrice;
        this.growthSpeed = _growthSpeed;
        this.levelReq = _levelReq;
        this.weight = _weight;
        this.type = _type;
        this.description = _description;
    }
};

[System.Serializable]
public class Upgrade {

    public string ID;
    public string name;
    public ResearchType typeR;
    public float amount;
    public ModType typeM;
    public int cost;
    public string description;

    public void Set(string _ID, string _name, ResearchType _typeR, float _amount, ModType _typeM, int _cost, string _description)
    {
        this.ID = _ID;
        this.name = _name;
        this.typeR = _typeR;
        this.amount = _amount;
        this.typeM = _typeM;
        this.cost = _cost;
        this.description = _description;
    }
};

public class ReadFile : MonoBehaviour
{
    public static void LoadItems(List<Item> list)
    {
        TextAsset listFile = Resources.Load<TextAsset>("Items");
        if (listFile == null)
        {
            Debug.LogError("Items.txt not found in Resources.");
            return;
        }

        string text = listFile.text;

        string[] lines = text.Split(
            new[] {'\r', '\n'}, 
            System.StringSplitOptions.RemoveEmptyEntries
        );

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];

            string[] parts = line.Split(
                (char[])null,
                9,
                System.StringSplitOptions.RemoveEmptyEntries
            );
            if (parts.Length < 8) continue;

            int.TryParse(parts[0], out int id);
            string name = parts[1].Replace('_', ' ');
            int.TryParse(parts[2], out int plant);
            int.TryParse(parts[3], out int sell);
            float.TryParse(parts[4], out float speed);
            int.TryParse(parts[5], out int requirement);
            float.TryParse(parts[6], out float weight);
            string type = parts[7];
            string description = parts[8];

            Item newItem =  new Item();
            newItem.Set(id, name, plant, sell, speed, requirement, weight, type, description);
            list.Add(newItem);
        }
    }

    public static void LoadBuildings(List<Item> list)
    {
        TextAsset buildingListFile = Resources.Load<TextAsset>("Builds");
        if (buildingListFile == null)
        {
            Debug.LogError("Builds.txt not found in Resources.");
            return;
        }

        string text = buildingListFile.text;

        string[] lines = text.Split(
            new[] {'\r', '\n'}, 
            System.StringSplitOptions.RemoveEmptyEntries
        );

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];

            string[] parts = line.Split(
                (char[])null,
                6,
                System.StringSplitOptions.RemoveEmptyEntries
            );
            if (parts.Length < 5) continue;

            int.TryParse(parts[0], out int id);
            string name = parts[1].Replace('_', ' ');
            int.TryParse(parts[2], out int plant);
            int.TryParse(parts[3], out int requirement);
            string type = parts[4];
            string description = parts[5];

            Item newItem = new Item();
            newItem.Set(id, name, plant, 0, 0, requirement, 0, type, description);
            list.Add(newItem);
        }
    }

    public static Sprite LoadIconSprite(string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
            return null;

        string iconName = itemName.Replace('_', ' ');
        
        Sprite icon = Resources.Load<Sprite>("Icons/" + iconName);
        if (icon == null)
            Debug.LogWarning($"Resource icon not found: Icons/{iconName}");

        return icon;
    }

    public static GameObject LoadPrefab(string itemName, params string[] searchFolders)
    {
        if (string.IsNullOrWhiteSpace(itemName))
            return null;

        string noSpaceName = itemName.Replace(" ", string.Empty);

        string[] nameVariants = new[] { itemName, noSpaceName };

        if (searchFolders != null)
        {
            foreach (string folder in searchFolders)
            {
                if (string.IsNullOrWhiteSpace(folder))
                    continue;

                string normalizedFolder = folder.Trim().Trim('/');
                if (normalizedFolder.StartsWith("Resources/", System.StringComparison.OrdinalIgnoreCase))
                    normalizedFolder = normalizedFolder.Substring("Resources/".Length);

                foreach (string name in nameVariants)
                {
                    GameObject prefab = Resources.Load<GameObject>($"{normalizedFolder}/{name}");
                    if (prefab != null)
                        return prefab;
                }
            }
        }

        foreach (string name in nameVariants)
        {
            GameObject prefab = Resources.Load<GameObject>(name);
            if (prefab != null)
                return prefab;
        }

        Debug.LogWarning($"[ReadFile] No prefab found for '{itemName}'");
        return null;
    }

    public static List<(string name, int quantity)> GetLevelUpRewards(int level)
    {
        List<(string name, int quantity)> rewards = new List<(string name, int quantity)>();
        TextAsset rewardsFile = Resources.Load<TextAsset>("LevelUpRewards");
        if (rewardsFile == null)
        {
            Debug.LogError("LevelUpRewards.txt not found in Resources.");
            return rewards;
        }

        string[] lines = rewardsFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        string targetLevelStr = level.ToString();

        foreach (string line in lines)
        {
            string[] parts = line.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0 && parts[0] == targetLevelStr)
            {
                for (int i = 1; i < parts.Length; i++)
                {
                    string reward = parts[i];
                    string name = reward;
                    int quantity = 0;

                    if (reward.Contains(","))
                    {
                        string[] rewardParts = reward.Split(',');
                        name = rewardParts[0];
                        int.TryParse(rewardParts[1], out quantity);
                    }

                    name = name.Replace('_', ' ');
                    rewards.Add((name, quantity));
                }
                break;
            }
        }

        return rewards;
    }

    public static void LoadUpgrades(List<Upgrade> list)
    {
        TextAsset upgradesFile = Resources.Load<TextAsset>("Upgrades");
        if (upgradesFile == null)
        {
            Debug.LogError("Upgrades.txt not found in Resources.");
            return;
        }

        string text = upgradesFile.text;
        string[] lines = text.Split(
            new[] { '\r', '\n' },
            System.StringSplitOptions.RemoveEmptyEntries
        );

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];

            string[] parts = line.Split(
                (char[])null,
                7,
                System.StringSplitOptions.RemoveEmptyEntries
            );
            if (parts.Length < 7) continue;

            string id = parts[0];
            string name = parts[1].Replace('_', ' ');
            Enum.TryParse(parts[2], true, out ResearchType typeR);
            float.TryParse(parts[3], out float amount);
            int.TryParse(parts[4], out int mod);
            ModType typeM = (mod == 1) ? ModType.PercentAdd : ModType.Flat;
            int.TryParse(parts[5], out int cost);
            string description = parts[6];

            Upgrade newUpgrade = new Upgrade();
            newUpgrade.Set(id, name, typeR, amount, typeM, cost, description);
            list.Add(newUpgrade);
        }
    }
}