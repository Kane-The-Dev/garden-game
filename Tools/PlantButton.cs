using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlantButton : MonoBehaviour
{
    public int plantID;
    public string plantName;
    public int plantPrice;
    public ButtonGroup myGroup;
    public Image myImage;
    [SerializeField] TextMeshProUGUI display, count;
    public Inventory inventory;

    public void Refresh()
    {
        if (inventory.myInventory.ContainsKey(plantName))
        {
            display.text = plantName;
            count.text = inventory.myInventory[plantName].ToString();
        }
        else
        {
            display.text = plantName;
            count.text = "0";
        }
    }

    public void OnClick()
    {
        FindObjectOfType<PlantManager>().ChangePlant(plantID);
        myGroup.OnClick(gameObject);
    }
}
