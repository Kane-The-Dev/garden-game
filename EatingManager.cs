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
    List<GameObject> spawnedFood = new List<GameObject>();
    public Queue<FoodDropRequest> q;
    [SerializeField] float delay, timer;

    [Header("Main Logic")]
    public float totalWeight;
    public float cooldownTimer;
    public Stat maxWeight = new Stat(30f), cooldown = new Stat(120f);
    public int accumulatedStonks_P, accumulatedStonks_O, accumulatedExp, accumulatedBonus;
    public Stat generalBonus = new Stat(0), plantBonus = new Stat(0), ovenBonus = new Stat(0);

    [Header("Truck")]
    [SerializeField] GameObject truck_kun;
    public GameObject myTruck;
    public Transform drop;
    public Stat truckCount = new Stat(1);
    public int truckLeft;
    
    [Header("Audio")]
    [SerializeField] AudioSource cashier;
    [SerializeField] AudioSource engine;
    [SerializeField] AudioClip cashIn, landing, starting;

    [Header("Display")]
    [SerializeField] RectTransform weightNeedle;
    [SerializeField] TextMeshProUGUI stonksDisplay;
    [SerializeField] TextMeshPro infoBoard;
    [SerializeField] UIParticleSystem coinBurst;

    Rigidbody rb;
    GameManager gm;

    void Awake()
    {
        timer = 0f; 
        delay = 0.2f;
        q = new Queue<FoodDropRequest>();
        truckLeft = Mathf.RoundToInt(truckCount.Value);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gm = GameManager.instance;
        SpawnTruck();
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

        if (cooldownTimer > 0) {
            if (infoBoard) 
                infoBoard.text = "Truck will be back after " + cooldownTimer.ToString("F0");

            cooldownTimer -= Time.deltaTime * GameManager.instance.timeControl;
        }

        if (myTruck == null && cooldownTimer <= 0) {
            if (infoBoard) 
                infoBoard.text = "Sell your stock here!";
            
            SpawnTruck();
        }

        if (gm.currentMode != 1) return;

        if (stonksDisplay) {
            string stonks = (accumulatedStonks_P + accumulatedStonks_O).ToString();
            if (accumulatedBonus > 0)
                stonks = stonks + '+' + accumulatedBonus;
            stonksDisplay.text = stonks + "G";
        }

        if (weightNeedle) {
            float targetRotZ = -140f + 100f * totalWeight / maxWeight.Value;
            Quaternion targetRot = Quaternion.Euler(0f, 0f, targetRotZ);
            weightNeedle.rotation = Quaternion.Lerp(weightNeedle.rotation, targetRot, 5f * Time.deltaTime);
        }
    }

    public void CalculateBonus()
    {
        accumulatedBonus = Mathf.RoundToInt(
            accumulatedStonks_P * (generalBonus.Value + plantBonus.Value) + 
            accumulatedStonks_O * (generalBonus.Value + ovenBonus.Value)
        );
    }

    void SpawnTruck()
    {
        truckLeft--;
        GameObject thisTruck = Instantiate(truck_kun, transform.position, transform.rotation);
        myTruck = thisTruck;
        drop = myTruck.transform.GetChild(5);
        rb = myTruck.GetComponent<Rigidbody>();
        engine = thisTruck.GetComponent<AudioSource>();
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

        GameObject obj = Instantiate(prefab, drop.position + Vector3.up * 4f, Quaternion.identity);

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
        engine.PlayOneShot(starting);
        foreach(GameObject obj in spawnedFood)
        {
            Growable fruit = obj.GetComponent<Growable>();
            if (fruit != null) 
                fruit.myAAS.mute = true; // mute fruits to prevent audio jumpscare
        }   

        for (int i = 0; i < 4; i++)
            myTruck.transform.GetChild(i).GetComponent<Spin>().speed = 180f;

        coinBurst.minCount = Mathf.Min(1 + (accumulatedStonks_P + accumulatedStonks_O) / 20, 30);
        coinBurst.maxCount = Mathf.Min(1 + (accumulatedStonks_P + accumulatedStonks_O) / 20, 30);
        coinBurst.Emission(0.05f);
            
        rb.constraints = RigidbodyConstraints.None;
        gm.inventory.coin += accumulatedStonks_P + accumulatedStonks_O;
        gm.inventory.exp += accumulatedExp;
        totalWeight = 0;
        accumulatedStonks_P = 0;
        accumulatedStonks_O = 0;
        accumulatedBonus = 0;

        Invoke("MoveTruck", 4f);
    }

    void MoveTruck()
    {
        Vector3 moveDir = transform.forward * 2000f + Vector3.up * 500f;
        rb.AddForce(moveDir * (1 + totalWeight * 0.2f), ForceMode.Impulse);

        foreach(GameObject obj in spawnedFood) Destroy(obj, 10f);
        spawnedFood.Clear();

        if (truckLeft > 1) {
            truckLeft--;
            cooldownTimer = 15f; // quick cooldown if substitute available
        }
        else {
            truckLeft = Mathf.RoundToInt(truckCount.Value);
            cooldownTimer = cooldown.Value;
        }
        
        Destroy(myTruck, 10f);
        myTruck = null;
    }

    void OnTriggerEnter(Collider col)
    {
        float magnitude = col.attachedRigidbody.velocity.magnitude;
        if (magnitude < 5f) return;

        if (col.CompareTag("Truck"))
        {
            engine.clip = landing;
            engine.Play();
        }
    }
}
