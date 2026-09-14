using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

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
    public int researchCredit = 0;
    [SerializeField] GameObject panel, holder;
    List<Upgrade> myUpgrades = new List<Upgrade>();
    [SerializeField] TextMeshProUGUI creditDisplay;

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

    [Header("Zoom Settings")]
    [SerializeField] float minScale = 0.5f;
    [SerializeField] float maxScale = 1.5f;
    [SerializeField] float zoomSpeed = 5f;
    float currentScaleFactor = 1f;

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
        
        ResearchCard[] cards = FindObjectsOfType<ResearchCard>(true);
        foreach (ResearchCard card in cards) card.Initialize(this);

        if (holder != null)
        {
            currentScaleFactor = holder.transform.localScale.x;
        }
    }

    void Update() 
    {
        if (Input.GetKeyDown(KeyCode.R) && gm.cam.movable == true)
        {
            OpenResearch();
        }

        if (panel != null && panel.activeSelf && holder != null)
        {
            creditDisplay.text = researchCredit.ToString();

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
            {
                currentScaleFactor += scroll * zoomSpeed;
                currentScaleFactor = Mathf.Clamp(currentScaleFactor, minScale, maxScale);
            }

            holder.transform.localScale = Vector3.Lerp(
                holder.transform.localScale, 
                Vector3.one * currentScaleFactor, 
                Time.deltaTime * 12f
            );
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
        if (trees == null || trees.Length == 0)
            Debug.LogWarning("ApplyGeneralGrowth failed: No Growable objects found.");
        else
            foreach (Growable tree in trees)
                if (!tree.isProduct)
                    tree.growthSpeed.AddModifier(mod);
    }

    public void ApplyPlantGrowth(Modifier mod)
    {
        plantGrowthMods.Add(mod);
        Growable[] trees = FindObjectsOfType<Growable>();
        if (trees == null || trees.Length == 0)
            Debug.LogWarning("ApplyPlantGrowth failed: No Growable objects found.");
        else
            foreach (Growable tree in trees)
                if (!tree.isProduct && !tree.isOven)
                    tree.growthSpeed.AddModifier(mod);
    }

    public void ApplyOvenGrowth(Modifier mod)
    {
        ovenGrowthMods.Add(mod);
        Growable[] trees = FindObjectsOfType<Growable>();
        if (trees == null || trees.Length == 0)
            Debug.LogWarning("ApplyOvenGrowth failed: No Growable objects found.");
        else
            foreach (Growable tree in trees)
                if (!tree.isProduct && tree.isOven)
                    tree.growthSpeed.AddModifier(mod);
    }

    public void ApplyGeneralProfit(Modifier mod)
    {
        generalProfitMods.Add(mod);
        EatingManager eater = gm.em;
        if (eater != null) 
        {
            eater.generalBonus.AddModifier(mod);
            eater.CalculateBonus();
        }
        else
            Debug.LogWarning("ApplyGeneralProfit failed: EatingManager is null.");
    }

    public void ApplyPlantProfit(Modifier mod)
    {
        plantProfitMods.Add(mod);
        EatingManager eater = gm.em;
        if (eater != null) 
        {
            eater.plantBonus.AddModifier(mod);
            eater.CalculateBonus();
        }
        else
            Debug.LogWarning("ApplyPlantProfit failed: EatingManager is null.");
    }

    public void ApplyOvenProfit(Modifier mod)
    {
        ovenProfitMods.Add(mod);
        EatingManager eater = gm.em;
        if (eater != null) 
        {
            eater.ovenBonus.AddModifier(mod);
            eater.CalculateBonus();
        }
        else
            Debug.LogWarning("ApplyOvenProfit failed: EatingManager is null.");
    }

    public void ApplyGoldenFruit(Modifier mod)
    {
        goldenFruitMods.Add(mod);
        Growable[] trees = FindObjectsOfType<Growable>();
        if (trees == null || trees.Length == 0)
            Debug.LogWarning("ApplyGoldenFruit failed: No Growable objects found.");
        else
            foreach (Growable tree in trees)
                if (!tree.isProduct && !tree.isOven)
                    tree.goldenChance.AddModifier(mod);
    }

    public void ApplyFruitCount(Modifier mod)
    {
        fruitCountMods.Add(mod);
        Growable[] trees = FindObjectsOfType<Growable>();
        if (trees == null || trees.Length == 0)
            Debug.LogWarning("ApplyFruitCount failed: No Growable objects found.");
        else
            foreach (Growable tree in trees)
                if (!tree.isProduct && !tree.isOven)
                    tree.blockedSlotCount.AddModifier(mod);
    }

    public void ApplyWindChance(Modifier mod)
    {
        windChanceMods.Add(mod);
        WindGenerator wind = FindObjectOfType<WindGenerator>();
        if (wind != null)
            wind.harvestChance.AddModifier(mod);
        else
            Debug.LogWarning("ApplyWindChance failed: WindGenerator not found.");
    }

    public void ApplyOvertime(Modifier mod)
    {
        overtimeMods.Add(mod);
        AutoHarvester[] gnomes = FindObjectsOfType<AutoHarvester>();
        if (gnomes == null || gnomes.Length == 0)
            Debug.LogWarning("ApplyOvertime failed: No AutoHarvester objects found.");
        else
            foreach (AutoHarvester gnome in gnomes)
                if (gnome != null) gnome.overtimeDuration.AddModifier(mod);
    }

    public void ApplyBetterTool(Modifier mod)
    {
        betterToolMods.Add(mod);
        PlantManager pm = gm.pm;
        if (pm != null)
            pm.toolMultiplier.AddModifier(mod);
        else
            Debug.LogWarning("ApplyBetterTool failed: PlantManager is null.");
    }

    public void ApplySaleCount(Modifier mod)
    {
        saleCountMods.Add(mod);
        ShopManager shop = gm.sm;
        if (shop != null)
        {
            shop.discountNumber.AddModifier(mod);
            shop.ShuffleDiscount();
        }
        else
            Debug.LogWarning("ApplySaleCount failed: ShopManager is null.");
    }

    public void ApplyDecorSale(Modifier mod)
    {
        decorSaleMods.Add(mod);
        ShopManager shop = gm.sm;
        if (shop != null)
        {
            shop.decorDiscount.AddModifier(mod);
            shop.SetPurchase(shop.selectedUI);
        }
        else
            Debug.LogWarning("ApplyDecorSale failed: ShopManager is null.");
    }

    public void ApplyShopDiscount(Modifier mod)
    {
        shopDiscountMods.Add(mod);
        ShopManager shop = gm.sm;
        if (shop != null)
        {
            shop.generalDiscount.AddModifier(mod);
            shop.SetPurchase(shop.selectedUI);
        }
        else
            Debug.LogWarning("ApplyShopDiscount failed: ShopManager is null.");
    }

    public void ApplyTruckWeight(Modifier mod)
    {
        truckWeightMods.Add(mod);
        EatingManager eater = gm.em;
        if (eater != null)
            eater.maxWeight.AddModifier(mod);
        else
            Debug.LogWarning("ApplyTruckWeight failed: EatingManager is null.");
    }

    public void ApplyTruckCooldown(Modifier mod)
    {
        truckCooldownMods.Add(mod);
        EatingManager eater = gm.em;
        if (eater != null) 
        {
            eater.cooldown.AddModifier(mod);
            eater.cooldownTimer += mod.value;
        }
        else
            Debug.LogWarning("ApplyTruckCooldown failed: EatingManager is null.");
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
        else
            Debug.LogWarning("ApplyTruckCount failed: EatingManager is null.");
    }
}
