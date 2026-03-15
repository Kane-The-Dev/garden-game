using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    int currentMode;
    [SerializeField] Transform garden, merchant, overview;
    public CameraMovement cam;
    public PlantManager pm;
    public ShopManager sm;
    public Inventory inventory;
    public EatingManager em;
    public GardenDecoration gd;
    [SerializeField] GameObject gardenTools, plantShop, foodStorage;

    public int timeControl;

    void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);
        else
            instance = this;

        currentMode = 0;
        cam = FindObjectOfType<CameraMovement>();
        pm = FindObjectOfType<PlantManager>();
        sm = FindObjectOfType<ShopManager>();
        inventory = FindObjectOfType<Inventory>();
    }

    public void ChangeMode(int mode) // 0 garden, 1 merchant, 2 overview
    {
        currentMode = mode;
        if (mode == 0) { // gardening

            cam.target = garden;
            cam.movable = true;

            gardenTools.SetActive(true);
            if (pm.mode == 0)
                plantShop.SetActive(true);

            foodStorage.SetActive(false);
        }
        else if (mode == 1) { // merchant

            cam.target = merchant;
            cam.movable = false;

            gardenTools.SetActive(false);
            plantShop.SetActive(false);
            foodStorage.SetActive(true);
        }
        else if (mode == 2) { // overview

            cam.target = overview;
            cam.movable = false;

            gardenTools.SetActive(false);
            plantShop.SetActive(false);
            foodStorage.SetActive(false);
        }
        cam.targetReached = false;
    }

    public void SwapMode()
    {
        if (currentMode == 0)
            ChangeMode(1);
        else if (currentMode == 1)
            ChangeMode(0);
    }
}
