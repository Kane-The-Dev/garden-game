using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AreYouSure : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] TextMeshProUGUI AYSMessage, inputWarning;
    [SerializeField] TMP_InputField input;
    public Action<bool, string> AYSAction;

    void Start()
    {
        GameManager.instance.AYSPanel = this;
    }

    public void OpenPanel(string msg, Action<bool, string> callback, bool requireInput = false, string defaultInput = "")
    {
        if (inputWarning) inputWarning.gameObject.SetActive(false);
        if (input)
        {
            input.gameObject.SetActive(requireInput);
            input.text = defaultInput;
        }

        panel.SetActive(true);
        AYSMessage.text = msg;
        AYSAction = callback;
    }

    public void Confirm(bool confirm)
    {
        string result = input != null ? input.text : "";

        if (confirm && input != null && input.gameObject.activeSelf)
        {
            if (string.IsNullOrWhiteSpace(result))
            {
                if (inputWarning) inputWarning.gameObject.SetActive(true);
                return;
            }
        }

        panel.SetActive(false);
        AYSAction?.Invoke(confirm, result);
    }
}
