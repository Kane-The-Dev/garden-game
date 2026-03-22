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
    public float totalWeight;
    public float cooldownTimer;

    [Header("Truck")]
    [SerializeField] GameObject truck_kun;
    GameObject myTruck;
    public Transform drop;
    
    [Header("Other")]
    public int accumulatedStonks;
    [SerializeField] TextMeshProUGUI stonksDisplay;
    [SerializeField] TextMeshPro infoBoard;

    [SerializeField] AudioSource cashier, engine;

    Rigidbody rb;

    void Start()
    {
        timer = 0f; 
        delay = 0.2f;
        q = new Queue<int>();
        rb = GetComponent<Rigidbody>();
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

        if (Input.GetKeyDown(KeyCode.B))
        {
            // ConfirmSale();
        }
    }

    public void ConfirmSale()
    {
        if (q.Count > 0 || cooldownTimer > 0) return;

        cooldownTimer = cooldown;

        cashier.Play();
        engine.Play();

        for (int i = 0; i < 4; i++)
            myTruck.transform.GetChild(i).GetComponent<Spin>().speed = 180f;
            
        rb.constraints = RigidbodyConstraints.None;
        GameManager.instance.inventory.coin += accumulatedStonks;
        totalWeight = 0;
        accumulatedStonks = 0;

        Invoke("MoveTruck", 4f);
    }

    void MoveTruck()
    {
        Vector3 moveDir = transform.forward * 2000f + Vector3.up * 500f;
        rb.AddForce(moveDir * (1 + totalWeight * 0.2f), ForceMode.Impulse);
        Debug.Log("Your force is " + (moveDir * (1 + totalWeight * 0.5f)));
        foreach(GameObject obj in spawnedFood) Destroy(obj, 10f);
        Destroy(myTruck, 10f);
    }

    void SpawnTruck()
    {
        GameObject thisTruck = Instantiate(truck_kun, transform.position, transform.rotation);
        myTruck = thisTruck;
        drop = myTruck.transform.GetChild(5);
        rb = myTruck.GetComponent<Rigidbody>();
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

    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Food"))
        {
            Debug.Log("Loaded more food!");
        }
    }
}
