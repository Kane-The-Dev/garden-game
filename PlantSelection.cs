using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlantSelection : MonoBehaviour
{
    public List<Item> foodList = new();
    [SerializeField] Transform selection;
    [SerializeField] GameObject plantButton;
    PlantManager pm;
    Inventory inventory;
    ButtonGroup group;

    void Start()
    {
        FindObjectOfType<ReadFile>().LoadItems(foodList);
        pm = GameManager.instance.pm;
        inventory = GameManager.instance.inventory;
        group = selection.gameObject.GetComponent<ButtonGroup>();
    }

    public void RefreshPlants()
    {
        group.buttons.Clear();
        foreach (Transform child in selection)
            Destroy(child.gameObject);

        PlantButton thisButton = null;
        foreach (var item in foodList.OrderBy(f => f.levelReq))
        {
            if (!inventory.myInventory.ContainsKey(item.name))
                continue;

            GameObject newItem = Instantiate(plantButton, selection);

            thisButton = newItem.transform.GetChild(0).GetComponent<PlantButton>();
            group.buttons.Add(thisButton.myImage);
            thisButton.myGroup = group;
            
            thisButton.plantID = item.ID;
            thisButton.plantName = item.name;
            thisButton.plantPrice = item.plantPrice;
            thisButton.inventory = inventory;
            thisButton.Refresh();

            if (thisButton.plantID == pm.plantID)
                thisButton?.OnClick();
        }

        // thisButton?.OnClick();
    }
}
