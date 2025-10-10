using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonGroup : MonoBehaviour
{
    public List<Image> buttons;
    [SerializeField] Color chosen, disabled;

    void Start()
    {
        buttons[0].color = chosen;
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
                button.color = chosen;
            }
        }
    }
}
