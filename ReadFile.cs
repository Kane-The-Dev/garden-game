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
                System.StringSplitOptions.RemoveEmptyEntries
            );
            if (parts.Length < 6) continue;

            int.TryParse(parts[0], out int id);
            string name = parts[1];
            int.TryParse(parts[2], out int plant);
            int.TryParse(parts[3], out int sell);
            float.TryParse(parts[4], out float speed);
            int.TryParse(parts[5], out int requirement);

            Item newItem =  new Item();
            newItem.Set(id, name, plant, sell, speed, requirement);
            list.Add(newItem);
        }
    }
}
