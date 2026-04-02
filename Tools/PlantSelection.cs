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
        plantGroup.buttons.Clear();
        foreach (Transform child in plantSelection)
            Destroy(child.gameObject);

        PlantButton thisButton = null;
        foreach (var item in foodList.OrderBy(f => f.levelReq))
        {
            if (!inventory.myInventory.ContainsKey(item.name))
                continue;

            GameObject newItem = Instantiate(plantButton, plantSelection);

            thisButton = newItem.transform.GetChild(0).GetComponent<PlantButton>();
            plantGroup.buttons.Add(thisButton.myImage);
            thisButton.myGroup = plantGroup;
            
            thisButton.plantID = item.ID;
            thisButton.plantName = item.name;
            thisButton.inventory = inventory;
            thisButton.Refresh();

            if (thisButton.plantID == pm.plantTool.plantID)
                thisButton?.OnClick();
        }
    }

    public void RefreshBuildings()
    {
        buildGroup.buttons.Clear();
        foreach (Transform child in buildSelection)
            Destroy(child.gameObject);

        BuildButton thisButton = null;
        foreach (var item in buildingList.OrderBy(f => f.levelReq))
        {
            if (!inventory.myInventory.ContainsKey(item.name))
                continue;

            GameObject newItem = Instantiate(buildButton, buildSelection);

            thisButton = newItem.transform.GetChild(0).GetComponent<BuildButton>();
            buildGroup.buttons.Add(thisButton.myImage);
            thisButton.myGroup = buildGroup;
            
            thisButton.buildID = item.ID;
            thisButton.buildName = item.name;
            thisButton.inventory = inventory;
            thisButton.Refresh();

            if (thisButton.buildID == pm.buildTool.buildID)
                thisButton?.OnClick();
        }
    }
}
