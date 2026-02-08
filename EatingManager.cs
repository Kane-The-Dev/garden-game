using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatingManager : MonoBehaviour
{
    [SerializeField] GameObject[] food;
    [SerializeField] Transform player;
    public Transform drop;
    [SerializeField] float delay, timer;
    public Queue<int> q;

    void Start()
    {
        timer = 0f; 
        delay = 0.15f;
        q = new Queue<int>();
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
    }

    void SpawnFood(int ID)
    {
        GameObject obj = Instantiate(food[ID], drop.position, Quaternion.identity);
            
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

        Destroy(obj, 5f);
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Food"))
        {
            player.localScale += new Vector3(0.001f, 0.001f, 0.001f);
            Destroy(collider.gameObject);
        }
    }
}
