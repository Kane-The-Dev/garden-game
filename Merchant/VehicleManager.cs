using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleManager : MonoBehaviour
{
    List<VehicleData> vehicleList = new List<VehicleData>();

    [SerializeField] GameObject[] myPrefabs, myPreviews;
    [SerializeField] GameObject vehicleDisplay;
    [SerializeField] Transform holder;
    public int ID;

    public List<VehicleDisplay> myDisplays = new List<VehicleDisplay>();
    EatingManager merchant;
    Inventory inv;

    void Awake() 
    {
        ReadFile.LoadVehicles(vehicleList);
    }

    void Start() 
    {
        merchant = GameManager.instance.em;
        inv = GameManager.instance.inventory;

        SpawnVehicleDisplays();

        if (myPrefabs != null && myPrefabs.Length > 0 && myPrefabs[0] != null)
            EquipVehicle(0);
    }

    void SpawnVehicleDisplays()
    {
        if (vehicleDisplay == null || holder == null) return;

        foreach (VehicleData data in vehicleList)
        {
            GameObject obj = Instantiate(vehicleDisplay, holder);
            VehicleDisplay display = obj.GetComponent<VehicleDisplay>();
            if (display != null)
            {
                display.Initialize(this, data);
                myDisplays.Add(display);
            }
        }
    }

    public void PreviewVehicle(int ID)
    {
        if (myPreviews == null) return;
        foreach (GameObject preview in myPreviews)
            if (preview != null) preview.SetActive(false);
            
        if (ID >= 0 && ID < myPreviews.Length && myPreviews[ID] != null)
            myPreviews[ID].SetActive(true);
    }

    public bool TryUnlockVehicle(int ID) 
    {
        if (ID < 0 || ID >= vehicleList.Count) return false;

        VehicleData data = vehicleList[ID];
        if (inv != null && inv.coin >= data.price && inv.level >= data.levelReq)
        {
            inv.coin -= data.price;
            return true;
        }
        Debug.Log("Cannot unlock");
        return false;
    }

    public void EquipVehicle(int ID)
    {
        if (ID < 0 || ID >= myPrefabs.Length || ID >= vehicleList.Count)
        {
            Debug.LogWarning("VehicleManager: ID out of bound!");
            return;
        }

        foreach (VehicleDisplay vd in myDisplays)
            vd.SetEquipped(vd.ID == ID);

        this.ID = ID;
        merchant.truck_kun = myPrefabs[ID];

        VehicleData data = vehicleList[ID];
        merchant.maxWeight.baseValue = data.maxWeight;
        merchant.cooldown.baseValue = data.cooldown;
        merchant.transportFee = data.tripFee;
    }
}
