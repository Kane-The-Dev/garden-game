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
        count.text = inventory.GetQuantity(key).ToString();
    }

    public void OnClick()
    {
        GameManager.instance.pm.ChangePlant(plantID);
        myGroup.OnClick(gameObject);
    }
}
