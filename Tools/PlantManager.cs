using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlantManager : MonoBehaviour
{
    Camera cam;
    GameManager gm;
    
    [Header("Preview")]
    public GameObject ring;
    public GameObject buildPreview;
    Renderer ringRender;
    [SerializeField] Color defaultRingColor;

    [Header("Layer Masks")]
    [SerializeField] LayerMask plantMask, groundMask, fruitMask, obstacleMask;
    
    
    [Header("Tools")]
    public int mode; // 0 = plant, 1 = build, 2 = water, 3 = harvest, 4 = chop
    public PlantTool plantTool;
    public BuildTool buildTool;
    public WaterTool waterTool;
    public HarvestTool harvestTool;
    public ChopTool chopTool;
    
    [Header("UI")]
    [SerializeField] Button[] modes = new Button[5];
    [SerializeField] Animator optionsAnimator;
    [SerializeField] TextMeshProUGUI gameTip;
    
    void Start()
    {
        mode = 0;
        cam = GetComponent<Camera>();
        gm = GameManager.instance;
        plantTool = GetComponent<PlantTool>();
        buildTool = GetComponent<BuildTool>();
        ringRender = ring.GetComponent<Renderer>();
        ringRender.material.color = defaultRingColor;
        ChangeMode(0);
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
            gameTip.gameObject.SetActive(false);
            if (buildPreview) buildPreview.SetActive(false);
            ring.SetActive(false);
            return;
        }

        gameTip.gameObject.SetActive(true);

        if (mode == 0 && plantTool.plantID >= 0)
        {
            ring.SetActive(true);
            plantTool.PlantCheck(ring, ray, groundMask, obstacleMask);
        }
        
        if (mode == 1 && buildTool.buildID >= 0)
        {
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
            if (mode > 1) {
                ring.SetActive(true);
                ringRender.material.color = defaultRingColor;
            }

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
            if (Input.GetKey(KeyCode.Q))
                buildTool.RotatePreview(buildPreview, 1);
            else if (Input.GetKey(KeyCode.E))
                buildTool.RotatePreview(buildPreview, -1);
    }

    public void ChangeMode(int newMode)
    {
        mode = newMode;
        
        switch (newMode)
        {
            case 0:
                if (plantTool.plantID >= 0) 
                    gameTip.text = "RMB to Plant";
                else 
                    gameTip.text = "Select a Plant";

                if (optionsAnimator) optionsAnimator.SetTrigger("plant");
                break;
            case 1:
                if (buildTool.buildID >= 0) 
                    gameTip.text = "E/R to Rotate\nRMB to Build";
                else
                    gameTip.text = "Select a Building";

                if (optionsAnimator) optionsAnimator.SetTrigger("build");
                break;
            default:
                gameTip.text = "RMB + Hold to Start";
                if (optionsAnimator) optionsAnimator.SetTrigger("close");
                break;

        }
    }

    public void ChangePlant(int newPlantID)
    {
        plantTool.plantID = newPlantID;

        if (newPlantID < 0) 
            gameTip.text = "Select a Plant";
        else if (GameManager.instance.inventory.foodList[newPlantID].type == "Oven") 
            gameTip.text = "RMB on an Oven to Start";
        else 
            gameTip.text = "RMB to Plant";
    }

    public void ChangeBuilding(int newBuildID)
    {
        if (newBuildID == buildTool.buildID) return;

        buildTool.buildID = newBuildID;

        if (buildPreview) Destroy(buildPreview);
        buildPreview = buildTool.SpawnPreview();

        if (buildTool.buildID >= 0) 
            gameTip.text = "Q/E to Rotate\nRMB to Build";
        else
            gameTip.text = "Select a Building";
    }
}
