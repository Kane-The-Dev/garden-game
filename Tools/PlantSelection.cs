using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlantSelection : MonoBehaviour
{
    List<Item> foodList = new(), buildingList = new();
    [SerializeField] Transform plantSelection, buildSelection;
    [SerializeField] GameObject plantButton, buildButton;
    [SerializeField] ButtonGroup plantGroup, buildGroup;
    PlantManager pm;
    Inventory inventory;
    
    void Start()
    {
        pm = GameManager.instance.pm;
        inventory = GameManager.instance.inventory;

        foodList = inventory.foodList;
        buildingList = inventory.buildingList;
    }

    public void RefreshPlants()
    {
        if (plantGroup != null)
            plantGroup.buttons.Clear();

        if (plantSelection != null)
        {
            foreach (Transform child in plantSelection)
                Destroy(child.gameObject);
        }

        if (inventory == null || plantButton == null || plantGroup == null)
            return;

        foreach (var item in foodList.OrderBy(f => f.levelReq))
        {
            if (!inventory.myInventory.ContainsKey(item.name))
                continue;

            GameObject newItem = Instantiate(plantButton, plantSelection);
            PlantButton thisButton = newItem.GetComponentInChildren<PlantButton>();
            if (thisButton == null)
                continue;

            plantGroup.buttons.Add(thisButton.myImage);
            thisButton.myGroup = plantGroup;
            
            thisButton.plantID = item.ID;
            thisButton.plantName = item.name;
            thisButton.inventory = inventory;
            thisButton.Refresh();

            if (pm != null && pm.plantTool != null && thisButton.plantID == pm.plantTool.plantID)
                thisButton.OnClick();
        }
    }

    public void RefreshBuildings()
    {
        if (buildGroup != null)
            buildGroup.buttons.Clear();

        if (buildSelection != null)
        {
            foreach (Transform child in buildSelection)
                Destroy(child.gameObject);
        }

        if (inventory == null || buildButton == null || buildGroup == null)
            return;

        foreach (var item in buildingList.OrderBy(f => f.levelReq))
        {
            if (!inventory.myInventory.ContainsKey(item.name))
                continue;

            GameObject newItem = Instantiate(buildButton, buildSelection);
            BuildButton thisButton = newItem.GetComponentInChildren<BuildButton>();
            if (thisButton == null)
                continue;

            buildGroup.buttons.Add(thisButton.myImage);
            thisButton.myGroup = buildGroup;
            
            thisButton.buildID = item.ID;
            thisButton.buildName = item.name;
            thisButton.inventory = inventory;
            thisButton.Refresh();

            if (pm != null && pm.buildTool != null && thisButton.buildID == pm.buildTool.buildID)
                thisButton.OnClick();
        }
    }
}
