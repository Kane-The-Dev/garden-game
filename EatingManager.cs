using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class EatingManager : MonoBehaviour
{
    [Header("Food Dropping")]
    [SerializeField] GameObject[] food;
    List<GameObject> spawnedFood = new List<GameObject>();
    public Queue<int> q;
    [SerializeField] float delay, timer, cooldown;
    public float totalWeight, maxWeight, cooldownTimer;
    public int accumulatedStonks;

    [Header("Truck")]
    [SerializeField] GameObject truck_kun;
    public GameObject myTruck;
    public Transform drop;
    
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

    void Start()
    {
        timer = 0f; 
        delay = 0.2f;
        q = new Queue<int>();
        rb = GetComponent<Rigidbody>();
        gm = GameManager.instance;
        SpawnTruck();
    }

    void Update()
    {
        if (timer > 0f) timer -= Time.deltaTime;
        else
        {
            timer = delay;
            if (q.Count > 0) 
            {
                SpawnFood(q.Dequeue());
            }
        }

        if (cooldownTimer > 0) {
            if (infoBoard) infoBoard.text = "Truck will be back after " + cooldownTimer.ToString("F0");
            cooldownTimer -= Time.deltaTime * GameManager.instance.timeControl;
        }
        if (myTruck == null && cooldownTimer <= 0) {
            if (infoBoard) infoBoard.text = "Sell your stock here!";
            SpawnTruck();
        }
        if (stonksDisplay) stonksDisplay.text = accumulatedStonks + "G";

        if (weightNeedle) {
            float targetRotZ = 210f + 120f * totalWeight / maxWeight;
            Quaternion targetRot = Quaternion.Euler(0f, 0f, targetRotZ);
            weightNeedle.rotation = Quaternion.Lerp(weightNeedle.rotation, targetRot, 5f * Time.deltaTime);
        }
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

        cooldownTimer = cooldown;

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

        coinBurst.minCount = Mathf.Min(1 + accumulatedStonks / 10, 30);
        coinBurst.maxCount = Mathf.Min(1 + accumulatedStonks / 10, 30);
        coinBurst.Emission(0.05f);
            
        rb.constraints = RigidbodyConstraints.None;
        GameManager.instance.inventory.coin += accumulatedStonks;
        totalWeight = 0;
        accumulatedStonks = 0;
        spawnedFood.Clear();

        Invoke("MoveTruck", 4f);
    }

    void MoveTruck()
    {
        Vector3 moveDir = transform.forward * 2000f + Vector3.up * 500f;
        rb.AddForce(moveDir * (1 + totalWeight * 0.2f), ForceMode.Impulse);
        Debug.Log("Your force is " + (moveDir * (1 + totalWeight * 0.5f)));
        foreach(GameObject obj in spawnedFood) Destroy(obj, 10f);
        Destroy(myTruck, 10f);
        myTruck = null;
    }

    void SpawnTruck()
    {
        GameObject thisTruck = Instantiate(truck_kun, transform.position, transform.rotation);
        myTruck = thisTruck;
        drop = myTruck.transform.GetChild(5);
        rb = myTruck.GetComponent<Rigidbody>();
        engine = thisTruck.GetComponent<AudioSource>();
    }

    void SpawnFood(int ID)
    {
        GameObject obj = Instantiate(food[ID], drop.position + Vector3.up * 4f, Quaternion.identity);
            
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
            obj.transform.localScale = Vector3.one * fruit.maxGrowth * 0.75f;
        }

        spawnedFood.Add(obj);
    }

    void OnTriggerEnter(Collider col)
    {
        float magnitude = col.attachedRigidbody.velocity.magnitude;
        if (magnitude < 5f) return;

        if (col.CompareTag("Food"))
        {
            Debug.Log("Loaded more food!");
        }

        if (col.CompareTag("Truck"))
        {
            engine.clip = landing;
            engine.Play();
        }
    }
}
