using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VehicleDisplay : MonoBehaviour
{
    public int ID;
    public bool isUnlocked, isEquipped;

    [SerializeField] GameObject details, priceTag;
    [SerializeField] TextMeshProUGUI nameDisplay, statsDisplay, description, buttonText, priceDisplay;
    VehicleManager manager;

    public void Initialize(VehicleManager manager, VehicleData data) 
    {
        this.manager = manager;
        this.ID = data.ID;
        
        if (nameDisplay != null) nameDisplay.text = data.name;
        if (statsDisplay != null)
        {
            statsDisplay.text = "Load capacity: " + data.maxWeight + "kg\n"
                + "Return time: " + data.cooldown + "s\n"
                + "Transport fee: " + data.tripFee + "G/trip";
        }
        if (description != null) description.text = data.description;
        if (priceDisplay) priceDisplay.text = data.price.ToString();

        if (this.ID == 0) isUnlocked = true;
        SetUnlocked(isUnlocked);
    }

    public void SetUnlocked(bool unlocked)
    {
        isUnlocked = unlocked;
        if (buttonText != null) buttonText.text = isUnlocked ? "Equip" : "Unlock";
        if (priceTag != null && isUnlocked) priceTag.SetActive(false);
    }

    public void SetEquipped(bool equipped)
    {
        if (!isUnlocked) return;
        
        isEquipped = equipped;
        if (isEquipped)
        {
            if (buttonText == null) return;
            
            buttonText.text = "Equipped";
            Button btn = buttonText.transform.parent.GetComponent<Button>();
            if (btn != null) btn.interactable = false;
        }
        else
        {
            if (buttonText == null) return;
            
            buttonText.text = "Equip";
            Button btn = buttonText.transform.parent.GetComponent<Button>();
            if (btn != null) btn.interactable = true;
        }
    }

    public void OpenDetails()
    {
        if (details != null)
            details.SetActive(!details.activeSelf);
        
        manager.PreviewVehicle(ID);

        foreach (VehicleDisplay vd in manager.myDisplays)
            if (vd != this) vd.details.SetActive(false);
    }

    public void OnClick()
    {
        if (manager == null) return;

        if (isUnlocked)
            manager.EquipVehicle(ID);
        else
        {
            if (manager.TryUnlockVehicle(ID))
            {
                SetUnlocked(true);
                manager.EquipVehicle(ID);
            }
        }
    }
}
