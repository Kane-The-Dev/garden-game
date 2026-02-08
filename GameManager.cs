using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    int mode; // 0 garden, 1 table
    [SerializeField] Transform garden, table;
    CameraMovement cam;
    public PlantManager pm;
    public ShopManager sm;
    public Inventory inventory;
    [SerializeField] GameObject gardenTools, plantShop, foodStorage;

    public int timeControl;

    void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);
        else
            instance = this;

        mode = 0;
        cam = FindObjectOfType<CameraMovement>();
        pm = FindObjectOfType<PlantManager>();
        sm = FindObjectOfType<ShopManager>();
        inventory = FindObjectOfType<Inventory>();
    }

    public void ChangeMode()
    {
        if (mode == 0) {
            mode = 1; // feeding

            cam.target = table;
            cam.movable = false;

            gardenTools.SetActive(false);
            plantShop.SetActive(false);
            foodStorage.SetActive(true);
        }
        else {
            mode = 0; // gardening

            cam.target = garden;
            cam.movable = true;

            gardenTools.SetActive(true);
            if (pm.mode == 0)
                plantShop.SetActive(true);

            foodStorage.SetActive(false);
        }
        cam.targetReached = false;
    }
}
