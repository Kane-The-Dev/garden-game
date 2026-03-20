using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadFile : MonoBehaviour
{
    [SerializeField] TextAsset listFile;

    public void LoadItems(List<Item> list)
    {
        string text = listFile.text;

        string[] lines = text.Split(
            new[] {'\r', '\n'}, 
            System.StringSplitOptions.RemoveEmptyEntries
        );

        foreach (string line in lines)
        {
            string[] parts = line.Split(
                (char[])null,
                9,
                System.StringSplitOptions.RemoveEmptyEntries
            );
            if (parts.Length < 8) continue;

            int.TryParse(parts[0], out int id);
            string name = parts[1];
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
}
