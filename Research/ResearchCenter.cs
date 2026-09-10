using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ResearchType
{
    growth_P, // plant
    growth_O, // oven

    profit_G,
    profit_P,
    profit_O,

    goldFruit,
    fruitCount,

    windChance,
    overtime,
    betterTool,

    saleCount,
    decorSale,
    shopDiscount,

    truckWeight,
    truckCooldown,
    truckCount
}

public class ResearchCenter : MonoBehaviour
{
    [SerializeField] GameObject panel;
    List<Upgrade> myUpgrades = new List<Upgrade>();
    List<ResearchCard> cards;

    public List<Modifier> generalGrowthMods = new List<Modifier>();
    public List<Modifier> plantGrowthMods = new List<Modifier>();
    public List<Modifier> ovenGrowthMods = new List<Modifier>();

    public List<Modifier> generalProfitMods = new List<Modifier>();
    public List<Modifier> plantProfitMods = new List<Modifier>();
    public List<Modifier> ovenProfitMods = new List<Modifier>();

    public List<Modifier> goldenFruitMods = new List<Modifier>();
    public List<Modifier> fruitCountMods = new List<Modifier>();

    public List<Modifier> windChanceMods = new List<Modifier>();
    public List<Modifier> overtimeMods = new List<Modifier>();
    public List<Modifier> betterToolMods = new List<Modifier>();

    public List<Modifier> saleCountMods = new List<Modifier>();
    public List<Modifier> decorSaleMods = new List<Modifier>();
    public List<Modifier> shopDiscountMods = new List<Modifier>();

    public List<Modifier> truckWeightMods = new List<Modifier>();
    public List<Modifier> truckCooldownMods = new List<Modifier>();
    public List<Modifier> truckCountMods = new List<Modifier>();

    GameManager gm;

    public Upgrade GetUpgrade(string ID)
    {
        return myUpgrades.FirstOrDefault(x => x.ID == ID);
    }

    void Awake()
    {
        ReadFile.LoadUpgrades(myUpgrades);
    }

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

    public void ApplyGeneralProfit(Modifier mod)
    {
        generalProfitMods.Add(mod);
    }

    public void ApplyPlantProfit(Modifier mod)
    {
        plantProfitMods.Add(mod);
    }

    public void ApplyOvenProfit(Modifier mod)
    {
        ovenProfitMods.Add(mod);
    }

    public void ApplyGoldenFruit(Modifier mod)
    {
        goldenFruitMods.Add(mod);
    }

    public void ApplyFruitCount(Modifier mod)
    {
        fruitCountMods.Add(mod);
        Growable[] trees = FindObjectsOfType<Growable>();
        foreach (Growable tree in trees)
        {
            if (!tree.isProduct && !tree.isOven)
                tree.blockedSlotCount.AddModifier(mod);
        }
    }

    public void ApplyWindChance(Modifier mod)
    {
        windChanceMods.Add(mod);
        WindGenerator wind = FindObjectOfType<WindGenerator>();
        if (wind != null)
            wind.harvestChance.AddModifier(mod);
    }

    public void ApplyOvertime(Modifier mod)
    {
        overtimeMods.Add(mod);
    }

    public void ApplyBetterTool(Modifier mod)
    {
        betterToolMods.Add(mod);
    }

    public void ApplySaleCount(Modifier mod)
    {
        saleCountMods.Add(mod);
    }

    public void ApplyDecorSale(Modifier mod)
    {
        decorSaleMods.Add(mod);
    }

    public void ApplyShopDiscount(Modifier mod)
    {
        shopDiscountMods.Add(mod);
    }

    public void ApplyTruckWeight(Modifier mod)
    {
        truckWeightMods.Add(mod);
        if (gm.em != null)
            gm.em.maxWeight.AddModifier(mod);
    }

    public void ApplyTruckCooldown(Modifier mod)
    {
        truckCooldownMods.Add(mod);
        if (gm.em != null)
            gm.em.cooldown.AddModifier(mod);
    }

    public void ApplyTruckCount(Modifier mod)
    {
        truckCountMods.Add(mod);
        EatingManager eater = gm.em;
        if (eater != null) 
        {
            eater.truckCount.AddModifier(mod);
            eater.truckLeft = Mathf.RoundToInt(eater.truckCount.Value);
        }
    }
}
