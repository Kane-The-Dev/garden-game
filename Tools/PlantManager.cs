using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlantManager : MonoBehaviour
{
    Camera cam;
    GameManager gm;

    public GameObject ring, buildPreview;
    [SerializeField] LayerMask plantMask, groundMask, fruitMask, obstacleMask;
    [SerializeField] Animator optionsAnimator;
    [SerializeField] Button[] modes = new Button[5];

    public PlantTool plantTool;
    public BuildTool buildTool;
    public WaterTool waterTool;
    public HarvestTool harvestTool;
    public ChopTool chopTool;
    
    public int mode; // 0 = plant, 1 = build, 2 = water, 3 = harvest, 4 = chop 
    
    void Start()
    {
        mode = 0;
        cam = GetComponent<Camera>();
        gm = GameManager.instance;
        plantTool = GetComponent<PlantTool>();
        buildTool = GetComponent<BuildTool>();
        ring.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f, 0.8f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Pressed 1");
            modes[0].onClick.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Pressed 2");
            modes[1].onClick.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("Pressed 3");
            modes[2].onClick.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("Pressed 4");
            modes[3].onClick.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Debug.Log("Pressed 5");
            modes[4].onClick.Invoke();
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (EventSystem.current.IsPointerOverGameObject() || gm.currentMode != 0)
        {
            if (buildPreview) buildPreview.SetActive(false);
            ring.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f, 0.8f);
            ring.SetActive(false);
            return;
        } 

        if (mode == 0 && plantTool.plantID >= 0)
        {
            ring.SetActive(true);
            plantTool.PlantCheck(ring, ray, groundMask, obstacleMask);
        }
        
        if (mode == 1 && buildTool.buildID >= 0)
        {
            // ring.SetActive(true);
            if (buildPreview) buildPreview.SetActive(true);
            buildTool.BuildCheck(buildPreview, ray, groundMask, obstacleMask);
        }

        if (Input.GetMouseButtonUp(0))
        {
            ring.SetActive(false);

            switch (mode) {
                case 2:
                    waterTool.StopWater();
                    break;
                case 3:
                    harvestTool.StopHarvest();
                    break;
            }
        }   

        if (Input.GetMouseButton(0))
        {
            switch (mode) {
                case 2:
                    waterTool.WaterTree(ring, ray, groundMask, fruitMask);
                    break;
                case 3:
                    harvestTool.HarvestTree(ring, ray, groundMask, plantMask);
                    break;
                case 4:
                    chopTool.ChopTree(ring, ray, groundMask, plantMask);
                    break;
            }
        }  
        
        if (Input.GetMouseButtonDown(0))
        {
            if (mode > 1) ring.SetActive(true);

            switch (mode) {
                case 0:
                    plantTool.PlantTree(ray, groundMask, obstacleMask);
                    break;
                case 1:
                    buildTool.BuildConfirm(ray, groundMask, obstacleMask);
                    break;
                case 2:
                    waterTool.StartWater();
                    break;
                case 3:
                    harvestTool.StartHarvest();
                    break;
            }
        }

        if (mode == 1)
            if (Input.GetKey(KeyCode.R))
                buildTool.RotatePreview(buildPreview, 1);
            else if (Input.GetKey(KeyCode.E))
                buildTool.RotatePreview(buildPreview, -1);
    }

    public void ChangeMode(int newMode)
    {
        mode = newMode;
        if (optionsAnimator)
        {
            switch (newMode)
            {
                case 0:
                    optionsAnimator.SetTrigger("plant");
                    break;
                case 1:
                    optionsAnimator.SetTrigger("build");
                    break;
                default:
                    optionsAnimator.SetTrigger("close");
                    break;

            }
        }
    }

    public void ChangePlant(int newPlantID)
    {
        plantTool.plantID = newPlantID;
    }

    public void ChangeBuilding(int newBuildID)
    {
        Debug.Log("Your buildID is " + newBuildID);

        if (newBuildID == buildTool.buildID) return;

        buildTool.buildID = newBuildID;

        if (buildPreview) Destroy(buildPreview);
        buildPreview = buildTool.SpawnPreview();
    }
}
