using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ResearchType
{
    growth_G, // general
    growth_P, // plant
    growth_O, // oven
    goldChance_P, // golden fruit
    goldChance_O, // golden baked
    maxTransport,
}

public class ResearchCenter : MonoBehaviour
{
    [SerializeField] GameObject panel;

    public List<Modifier> generalGrowthMods = new List<Modifier>();
    public List<Modifier> plantGrowthMods = new List<Modifier>();
    public List<Modifier> ovenGrowthMods = new List<Modifier>();
    public List<Modifier> goldenFruitMods = new List<Modifier>();
    public List<Modifier> goldenBakedMods = new List<Modifier>();
    public List<Modifier> windChanceMods = new List<Modifier>();
    public List<Modifier> maxTransportMods = new List<Modifier>();
    public List<Modifier> bonusProfitMods = new List<Modifier>();
    public int moreFruitCount;
    public int moreBakedCount;

    GameManager gm;

    void Start()
    {
        gm = GameManager.instance;
    }

    void Update() 
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            OpenResearch();
        }
    }

    public void OpenResearch()
    {
        panel.SetActive(true);
        gm.cam.movable = false;
    }

    public void CloseResearch()
    {
        panel.SetActive(false);
        gm.cam.movable = true;
    }
    
    public void ApplyGeneralGrowth(Modifier mod)
    {
        generalGrowthMods.Add(mod);
        Growable[] trees = FindObjectsOfType<Growable>();
        foreach (Growable tree in trees)
        {
            if (!tree.isProduct)
                tree.growthSpeed.AddModifier(mod);
        }
    }

    public void ApplyPlantGrowth(Modifier mod)
    {
        plantGrowthMods.Add(mod);
        Growable[] trees = FindObjectsOfType<Growable>();
        foreach (Growable tree in trees)
        {
            if (!tree.isProduct && !tree.isOven)
                tree.growthSpeed.AddModifier(mod);
        }
    }

    public void ApplyOvenGrowth(Modifier mod)
    {
        ovenGrowthMods.Add(mod);
        Growable[] trees = FindObjectsOfType<Growable>();
        foreach (Growable tree in trees)
        {
            if (!tree.isProduct && tree.isOven)
                tree.growthSpeed.AddModifier(mod);
        }
    }

    public void ApplyMerchantBuffs()
    {
        
    }
}
