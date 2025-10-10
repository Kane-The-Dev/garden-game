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
    [SerializeField] TextMeshProUGUI display;

    void Start()
    {
        display.text = plantName + '\n' + plantPrice.ToString() + 'G';
    }

    public void OnClick()
    {
        FindObjectOfType<PlantManager>().ChangePlant(plantID);
        myGroup.OnClick(gameObject);
    }
}
