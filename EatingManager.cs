using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EatingManager : MonoBehaviour
{
    [SerializeField] GameObject[] food, spawnedFood;
    [SerializeField] GameObject truck_kun;
    GameObject myTruck;
    public Transform drop;
    [SerializeField] float delay, timer, cooldown, cooldownTimer;
    public Queue<int> q;
    public int accumulatedStonks;
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

        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime * GameManager.instance.timeControl;
        if (myTruck == null && cooldownTimer <= 0) SpawnTruck();

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (q.Count > 0 || cooldownTimer > 0) return;
            
            rb.constraints = RigidbodyConstraints.None;
            rb.AddForce(transform.forward * 1500f + Vector3.up * 200f, ForceMode.Impulse);
            GameManager.instance.inventory.coin += accumulatedStonks;
            accumulatedStonks = 0;
            cooldownTimer = cooldown;

            foreach(GameObject obj in spawnedFood) Destroy(obj, 10f);
            Destroy(myTruck, 10f);
        }
    }

    void SpawnTruck()
    {
        GameObject thisTruck = Instantiate(truck_kun, transform.position, transform.rotation);
        myTruck = thisTruck;
        drop = myTruck.transform.GetChild(0);
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

        spawnedFood.Append(obj);

        // Destroy(obj, 5f);
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Food"))
        {
            Debug.Log("Loaded more food!");
            //player.localScale += new Vector3(0.001f, 0.001f, 0.001f);
            //Destroy(collider.gameObject);
        }
    }
}
