using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AreYouSure : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] TextMeshProUGUI AYSMessage;
    Action<bool> AYSAction;

    void Start()
    {
        GameManager.instance.AYSPanel = this;
    }

    public void OpenPanel(string msg, Action<bool> callback)
    {
        panel.SetActive(true);
        AYSMessage.text = msg;
        AYSAction = callback;
    }

    public void Confirm(bool confirm)
    {
        panel.SetActive(false);
        AYSAction?.Invoke(confirm);
    }
}
