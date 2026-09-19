using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

[System.Serializable]
public struct FoodDropRequest
{
    public int ID;
    public bool isGolden;

    public FoodDropRequest(int ID, bool isGolden = false)
    {
        this.ID = ID;
        this.isGolden = isGolden;
    }
}

public class EatingManager : MonoBehaviour
{
    [Header("Food Dropping")]
    public List<GameObject> spawnedFood = new List<GameObject>();
    public Queue<FoodDropRequest> q;
    public List<FoodDropRequest> currentRequests = new List<FoodDropRequest>();
    [SerializeField] float delay, timer;
    public bool dealCompleted = false;

    [Header("Main Logic")]
    public float totalWeight;
    public float cooldownTimer;
    public int transportFee;
    public Stat maxWeight = new Stat(30f), cooldown = new Stat(120f);
    public int accumulatedStonks_P, accumulatedStonks_O, accumulatedExp, accumulatedBonus;
    public Stat generalBonus = new Stat(0), plantBonus = new Stat(0), ovenBonus = new Stat(0);

    [Header("Truck")]
    public GameObject truck_kun; // selected vehicle
    public GameObject myTruck;
    public Vehicle vehicle;
    public Stat truckCount = new Stat(1);
    public int truckLeft;
    Vector3 moveDir;
    
    [Header("Audio")]
    [SerializeField] AudioSource cashier;
    [SerializeField] AudioClip cashIn;

    [Header("Display")]
    [SerializeField] RectTransform weightNeedle;
    [SerializeField] TextMeshProUGUI stonksDisplay, feeDisplay;
    [SerializeField] TextMeshPro infoBoard;
    [SerializeField] UIParticleSystem coinBurst;

    GameManager gm;

    public bool IsTruckAvailable()
    {
        return myTruck != null
            && dealCompleted == false
            && cooldownTimer <= 0f
            && myTruck.transform.position.y <= 3f;
    }

    public void CalculateBonus()
    {
        accumulatedBonus = Mathf.RoundToInt(
            accumulatedStonks_P * (generalBonus.Value + plantBonus.Value) + 
            accumulatedStonks_O * (generalBonus.Value + ovenBonus.Value)
        );
    }

    void Awake()
    {
        timer = 0f; 
        delay = 0.2f;
        q = new Queue<FoodDropRequest>();
        truckLeft = Mathf.RoundToInt(truckCount.Value);
    }

    void Start()
    {
        gm = GameManager.instance;
    }

    void Update()
    {
        if (q.Count > 0)
        {
            if (timer > 0f) 
                timer -= Time.deltaTime;
            else
            {
                timer = delay;
                SpawnFood(q.Dequeue());
            }
        }

        // Truck cooldown & respawn
        if (cooldownTimer > 0f) {
            cooldownTimer -= Time.deltaTime * GameManager.instance.timeControl;
            if (cooldownTimer < 0f) cooldownTimer = 0f;

            if (infoBoard) 
                infoBoard.text = "Truck will be back after " + cooldownTimer.ToString("F0");
        }
        else if (myTruck == null && !dealCompleted) {
            // No truck -> bring in next truck
            SpawnTruck();

            if (infoBoard)
                infoBoard.text = "Sell your stock here!";
        }

        // Update UI if in selling panel
        if (gm.currentMode != 1) return;

        if (stonksDisplay) {
            string stonks = (accumulatedStonks_P + accumulatedStonks_O).ToString();
            if (accumulatedBonus > 0)
                stonks = stonks + '+' + accumulatedBonus;
            stonksDisplay.text = stonks + "G";
        }

        if (feeDisplay) {
            feeDisplay.text = "-" + transportFee + "G";
        }

        if (weightNeedle) {
            float targetRotZ = -140f + 100f * totalWeight / maxWeight.Value;
            Quaternion targetRot = Quaternion.Euler(0f, 0f, targetRotZ);
            weightNeedle.rotation = Quaternion.Lerp(weightNeedle.rotation, targetRot, 5f * Time.deltaTime);
        }
    }

    void SpawnFood(FoodDropRequest request)
    {
        Inventory inventory = gm.inventory;
        Item item = inventory.foodList.Find(f => f.ID == request.ID);
        if (item == null)
        {
            Debug.LogWarning($"SpawnFood: no item with ID {request.ID} in foodList.");
            return;
        }

        GameObject prefab = inventory.LoadProductPrefab(item.name);
        if (prefab == null)
            return;

        GameObject obj = Instantiate(
            prefab, 
            vehicle.drop.position + Vector3.up * 5f, 
            Quaternion.identity
        );

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.None;
            rb.useGravity = true;
        }

        Collider col = obj.GetComponent<Collider>();
        col.isTrigger = false;

        Growable fruit = obj.GetComponent<Growable>();
        if (fruit != null)
        {
            fruit.chopped = true;
            obj.transform.localScale = 0.8f * Vector3.one * fruit.maxGrowth;
            if (request.isGolden)
            {
                fruit.isGolden = true;
                if (gm.pm.gold != null)
                {
                    MeshRenderer mr = obj.GetComponentInChildren<MeshRenderer>();
                    if (mr != null) mr.material = gm.pm.gold;
                }
            }
        }

        spawnedFood.Add(obj);
        accumulatedExp += 2;
    }

    public void SpawnTruck()
    {
        if (cooldownTimer > 0f) return;

        myTruck = Instantiate(truck_kun, transform.position, transform.rotation);
        if (myTruck != null)
            vehicle = myTruck.GetComponent<Vehicle>();
    }
    
    public void ConfirmSale()
    {
        if (q.Count > 0) {
            gm.mouse.myEffect.Burst("Food is loading!", new Color32(0xDE, 0x55, 0x57, 0xFF));
            gm.am.PlayUISoundEffect(6);
            return;
        }

        if (cooldownTimer > 0) {
            gm.mouse.myEffect.Burst("Truck unavailable!", new Color32(0xDE, 0x55, 0x57, 0xFF));
            gm.am.PlayUISoundEffect(6);
            return;
        }

        if (accumulatedStonks_P + accumulatedStonks_O <= 0) {
            gm.mouse.myEffect.Burst("Nothing to sell!", new Color32(0xDE, 0x55, 0x57, 0xFF));
            gm.am.PlayUISoundEffect(6);
            return;
        }

        cashier.PlayOneShot(cashIn);

        if (vehicle != null)
            vehicle.StartEngine();
    
        foreach(GameObject obj in spawnedFood)
        {
            Growable fruit = obj.GetComponent<Growable>();
            if (fruit != null) 
                fruit.myAAS.mute = true; // mute fruits to prevent audio jumpscare
        }

        coinBurst.minCount = Mathf.Min(1 + (accumulatedStonks_P + accumulatedStonks_O) / 20, 30);
        coinBurst.maxCount = Mathf.Min(1 + (accumulatedStonks_P + accumulatedStonks_O) / 20, 30);
        coinBurst.Emission(0.05f);
            
        gm.inventory.coin += (accumulatedStonks_P + accumulatedStonks_O + accumulatedBonus) - transportFee;
        gm.inventory.exp += accumulatedExp;
        totalWeight = 0;
        accumulatedStonks_P = 0;
        accumulatedStonks_O = 0;
        accumulatedBonus = 0;
        accumulatedExp = 0;

        moveDir = (vehicle.rb.mass + totalWeight) * (transform.forward * 32f + Vector3.up * 8f);
        currentRequests.Clear();

        truckLeft--;
        dealCompleted = true;

        Destroy(myTruck, 15f);
        myTruck = null;
        Invoke(nameof(MoveTruck), 4f);
    }

    void MoveTruck()
    {
        vehicle.Move(moveDir);
        vehicle = null;

        foreach(GameObject obj in spawnedFood) Destroy(obj, 10f);
        spawnedFood.Clear();

        if (truckLeft > 0) {
            cooldownTimer = 15f; // quick cooldown if substitute available
        }
        else {
            truckLeft = Mathf.RoundToInt(truckCount.Value);
            cooldownTimer = cooldown.Value;
        }
        
        dealCompleted = false;
    }

    void OnTriggerEnter(Collider col)
    {
        float magnitude = col.attachedRigidbody.velocity.magnitude;
        if (magnitude < 5f) return;

        if (col.CompareTag("Truck"))
        {
            vehicle.CollideGround();
        }
    }
}
