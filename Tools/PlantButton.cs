using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlantButton : MonoBehaviour
{
    public int plantID;
    public string plantName;
    public ButtonGroup myGroup;
    public Image myImage;
    [SerializeField] TextMeshProUGUI display, count;
    public Inventory inventory;

    public void Refresh()
    {
        string key = plantName;
        display.text = Inventory.GetProductName(plantName);
        if (inventory.myInventory.ContainsKey(key))
            count.text = inventory.myInventory[key].ToString();
        else
            count.text = "0";
    }

    public void OnClick()
    {
        GameManager.instance.pm.ChangePlant(plantID);
        myGroup.OnClick(gameObject);
    }
}
