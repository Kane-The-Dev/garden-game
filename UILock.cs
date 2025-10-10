using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UILock : MonoBehaviour
{
    public int levelRequirement;
    [SerializeField] TextMeshProUGUI display;

    void Start()
    {
        display.text = "Level " + levelRequirement.ToString();
        display.fontSize = 20f;
    }

    public void Unlock()
    {
        int currentLevel = FindObjectOfType<Inventory>().level;
        if (currentLevel >= levelRequirement)
        Destroy(gameObject);
        else
        Debug.Log("Chưa đủ kinh nghiệm");
    }
}
