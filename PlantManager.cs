using UnityEngine;
using UnityEngine.EventSystems;

public class PlantManager : MonoBehaviour
{
    Camera cam;
    
    [SerializeField] GameObject[] plants, products, decorations;
    [SerializeField] LayerMask plantMask, groundMask, fruitMask;

    public PlantTool plantTool;
    public WaterTool waterTool;
    public HarvestTool harvestTool;
    public ChopTool chopTool;
    
    public int mode; // 0 = plant, 1 = water, 2 = harvest, 3 = chop
    public int plantID;
    float maxDistance = 100f;

    void Start()
    {
        cam = GetComponent<Camera>();
        
        plantTool = GetComponent<PlantTool>();
        waterTool = GetComponent<WaterTool>();
        harvestTool = GetComponent<HarvestTool>();
        chopTool = GetComponent<ChopTool>();

        InitializeGarden();
    }

    void Update()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonUp(0) && mode == 1)
            waterTool.StopWater();

        if (Input.GetMouseButton(0) && mode == 1)
            waterTool.WaterTree(ray, groundMask, fruitMask);
        
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            return;

            switch (mode) {
                case 0:
                    plantTool.PlantTree(plantID, ray, groundMask);
                    break;               
                case 1:
                    waterTool.StartWater();
                    break;
                case 2:
                    harvestTool.HarvestTree(ray, plantMask);
                    break;
                case 3:
                    harvestTool.HarvestTree(ray, plantMask);
                    chopTool.ChopTree(ray, plantMask);
                    break;
            }
        }
    }

    void InitializeGarden()
    {
        int n = Random.Range(20, 30);
        float randomX, randomZ;

        for (int i = 0; i < n; i++)
        {
            randomX = Random.Range(-3f, 23f);
            randomZ = Random.Range(-3f, 23f);

            Vector3 point = new Vector3(randomX, 10f, randomZ);

            if (Physics.Raycast(point, Vector3.down, out RaycastHit hit, maxDistance, groundMask))
                
                if (hit.collider.CompareTag("Obstacle")) {
                    i--;
                    continue;
                }

                Instantiate(
                    decorations[Random.Range(0, decorations.Length)], 
                    hit.point, 
                    Quaternion.Euler(0f, Random.Range(-90f, 90f), 0f)
                );
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
