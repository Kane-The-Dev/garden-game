using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    int mode; // 0 garden, 1 table
    [SerializeField] Transform garden, table;
    CameraMovement cam;

    [SerializeField] GameObject gardenTools, plantShop, foodStorage; 

    void Start()
    {
        mode = 0;
        cam = FindObjectOfType<CameraMovement>();
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
            if (FindObjectOfType<PlantManager>().mode == 0) plantShop.SetActive(true);
            foodStorage.SetActive(false);
        }
        cam.targetReached = false;
    }
}
