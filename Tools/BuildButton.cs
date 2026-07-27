using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildButton : MonoBehaviour
{
    public int buildID;
    public string buildName;
    public ButtonGroup myGroup;
    public Image myImage;
    [SerializeField] TextMeshProUGUI display, count;
    public Inventory inventory;

    public void Refresh()
    {
        display.text = buildName;
        count.text = inventory.GetQuantity(buildName).ToString();
    }

    public void OnClick()
    {
        GameManager.instance.pm.ChangeBuilding(buildID);
        myGroup.OnClick(gameObject);
    }
}
