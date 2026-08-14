using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadFile : MonoBehaviour
{
    [SerializeField] TextAsset listFile, buildingListFile;

    public void LoadItems(List<Item> list)
    {
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

    public void LoadBuildings(List<Item> list)
    {
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
}