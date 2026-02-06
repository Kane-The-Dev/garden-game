using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonGroup : MonoBehaviour
{
    public List<Image> buttons;
    [SerializeField] Color selected, disabled;

    void Start()
    {
        UpdateSelected(0);
    }

    public void UpdateSelected(int i)
    {
        Debug.Log("Selected plantID is " + i);
        if (buttons.Count > i)
            buttons[i].color = selected;
    }
    
    public void OnClick(GameObject pressed)
    {
        foreach(Image button in buttons)
        {
            if (button.gameObject != pressed)
            {
                button.color = disabled;
            }
            else
            {
                button.color = selected;
            }
        }
    }
}
