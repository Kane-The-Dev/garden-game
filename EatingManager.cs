using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class EatingManager : MonoBehaviour
{
    [Header("Food Dropping")]
    List<GameObject> spawnedFood = new List<GameObject>();
    public Queue<int> q;
    [SerializeField] float delay, timer;
    public float totalWeight, cooldownTimer;
    public Stat maxWeight = new Stat(30f), cooldown = new Stat(120f);
    public int accumulatedStonks, accumulatedExp;

    [Header("Truck")]
    [SerializeField] GameObject truck_kun;
    public GameObject myTruck;
    public Transform drop;
    public Stat truckCount = new Stat(1);
    public int truckLeft;
    
    [Header("Audio")]
    [SerializeField] AudioSource cashier;
    [SerializeField] AudioSource engine;
    [SerializeField] AudioClip cashIn, error, landing, starting;

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
        q = new Queue<int>();
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

        if (stonksDisplay) stonksDisplay.text = accumulatedStonks + "G";

        if (weightNeedle) {
            float targetRotZ = -140f + 100f * totalWeight / maxWeight.Value;
            Quaternion targetRot = Quaternion.Euler(0f, 0f, targetRotZ);
            weightNeedle.rotation = Quaternion.Lerp(weightNeedle.rotation, targetRot, 5f * Time.deltaTime);
        }
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
    
    void SpawnFood(int ID)
    {
        Inventory inventory = gm.inventory;
        Item item = inventory.foodList.Find(f => f.ID == ID);
        if (item == null)
        {
            Debug.LogWarning($"SpawnFood: no item with ID {ID} in foodList.");
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

        Growable fruit = obj.GetComponent<Growable>();
        if (fruit != null)
        {
            fruit.chopped = true;
            obj.transform.localScale = 0.8f * Vector3.one * fruit.maxGrowth;
        }

        spawnedFood.Add(obj);

        accumulatedExp += 2;
    }

    public void ConfirmSale()
    {
        if (q.Count > 0) {
            gm.mouse.myEffect.Burst("Food is loading!");
            cashier.PlayOneShot(error);
            return;
        }

        if (cooldownTimer > 0) {
            gm.mouse.myEffect.Burst("Wait for truck!");
            cashier.PlayOneShot(error);
            return;
        }

        if (accumulatedStonks <= 0) {
            gm.mouse.myEffect.Burst("Nothing to sell!");
            cashier.PlayOneShot(error);
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

        coinBurst.minCount = Mathf.Min(1 + accumulatedStonks / 20, 30);
        coinBurst.maxCount = Mathf.Min(1 + accumulatedStonks / 20, 30);
        coinBurst.Emission(0.05f);
            
        rb.constraints = RigidbodyConstraints.None;
        gm.inventory.coin += accumulatedStonks;
        gm.inventory.exp += accumulatedExp;
        totalWeight = 0;
        accumulatedStonks = 0;

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
