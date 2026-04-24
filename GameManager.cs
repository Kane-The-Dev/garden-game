using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Modes")]
    public int currentMode;
    [SerializeField] Transform garden, merchant, overview;

    [Header("Managers")]
    public CameraMovement cam;
    public PlantManager pm;
    public ShopManager sm;
    public Inventory inventory;
    public EatingManager em;
    public FenceManager fence;
    public AudioManager am;
    public FollowMouse mouse;
    public AreYouSure AYSPanel;

    [Header("UI Elements")]
    [SerializeField] GameObject gardenTools;
    [SerializeField] GameObject foodStorage, weightDisplay;

    public int timeControl;

    void Awake()
    {
        if (instance != null && instance != this)
            Destroy(gameObject);
        else
            instance = this;

        DontDestroyOnLoad(gameObject);

        currentMode = 0;

        if (!cam) cam = FindObjectOfType<CameraMovement>();
        if (!pm) pm = FindObjectOfType<PlantManager>();
        if (!sm) sm = FindObjectOfType<ShopManager>();
        if (!inventory) inventory = FindObjectOfType<Inventory>();
        if (!em) em = FindObjectOfType<EatingManager>();
        if (!fence) fence = FindObjectOfType<FenceManager>();
        if (!am) am = FindObjectOfType<AudioManager>();
        if (!mouse) mouse = FindObjectOfType<FollowMouse>();
    }

    void Start()
    {
        am = AudioManager.instance;
    }

    public void ChangeMode(int mode) // 0 garden, 1 merchant, 2 overview
    {
        currentMode = mode;
        if (mode == 0) { // gardening

            cam.target = garden;
            cam.movable = true;

            gardenTools.SetActive(true);
            pm.ChangeMode(pm.mode);
            foodStorage.SetActive(false);
            weightDisplay.SetActive(false);
        }
        else if (mode == 1) { // merchant

            cam.target = merchant;
            cam.movable = false;

            gardenTools.SetActive(false);
            foodStorage.SetActive(true);
            weightDisplay.SetActive(true);
        }
        else if (mode == 2) { // overview

            cam.target = overview;
            cam.movable = false;

            gardenTools.SetActive(false);
            foodStorage.SetActive(false);
            weightDisplay.SetActive(false);
        }
        cam.targetReached = false;
        am.PlayUISoundEffect(5); // whoosh sound
    }

    public void SwapMode()
    {
        if (currentMode == 0)
            ChangeMode(1);
        else if (currentMode == 1)
            ChangeMode(0);
    }
}
