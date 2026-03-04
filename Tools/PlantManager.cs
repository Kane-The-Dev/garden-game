using UnityEngine;
using UnityEngine.EventSystems;

public class PlantManager : MonoBehaviour
{
    Camera cam;

    [SerializeField] public GameObject ring;
    [SerializeField] private LayerMask plantMask, groundMask, fruitMask, obstacleMask;

    public PlantTool plantTool;
    public WaterTool waterTool;
    public HarvestTool harvestTool;
    public ChopTool chopTool;
    
    public int mode; // 0 = plant, 1 = water, 2 = harvest, 3 = chop
    public int plantID;

    void Start()
    {
        mode = 0;
        plantID = -1;

        cam = GetComponent<Camera>();
        
        plantTool = GetComponent<PlantTool>();

        ring.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f, 0.8f);
    }

    void Update()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (EventSystem.current.IsPointerOverGameObject())
        {
            ring.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f, 0.8f);
            ring.SetActive(false);
            return;
        }

        if (mode == 0)
        {
            ring.SetActive(true);
            plantTool.PlantCheck(ring, ray, groundMask, obstacleMask);
        }

        if (Input.GetMouseButtonUp(0))
        {
            ring.SetActive(false);

            switch (mode) {
                case 1:
                    waterTool.StopWater();
                    break;
                case 2:
                    harvestTool.StopHarvest();
                    break;
            }
        }   

        if (Input.GetMouseButton(0))
        {
            switch (mode) {
                case 1:
                    waterTool.WaterTree(ring, ray, groundMask, fruitMask);
                    break;
                case 2:
                    harvestTool.HarvestTree(ring, ray, groundMask, plantMask);
                    break;
                case 3:
                    chopTool.ChopTree(ring, ray, groundMask, plantMask);
                    break;
            }
        }
            
        
        if (Input.GetMouseButtonDown(0))
        {
            switch (mode) {
                case 0:
                    if (plantID == -1) break;
                    plantTool.PlantTree(plantID, ray, groundMask, obstacleMask);
                    break;
                case 1:
                    ring.SetActive(true);
                    waterTool.StartWater();
                    break;
                case 2:
                    ring.SetActive(true);
                    harvestTool.StartHarvest();
                    break;
                default:
                    ring.SetActive(true);
                    break;
            }
        }
    }

    public void ChangeMode(int newMode)
    {
        mode = newMode;
    }

    public void ChangePlant(int newPlantID)
    {
        plantID = newPlantID;
    }
}
