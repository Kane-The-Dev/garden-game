using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Growable : MonoBehaviour
{
    [Header("Growth")]
    public float maxGrowth;
    public float growthIndex, growthSpeed = 1f, multiplier;

    [Header("Tree")]
    public GameObject product, leaf;
    public Transform[] slots;
    public bool reproductive = false;
    public int fruitCount;
    
    [Header("Product")]
    public bool isProduct;
    public int productID;

    [Header("Other")]
    public bool chopped = false;
    [SerializeField] float shakeAmplitude;
    Vector2 shakeDirection;

    void Awake()
    {
        growthIndex = 0.2f;
        multiplier = 1f;
        transform.localScale = new Vector3(1,1,1) * 0.2f * maxGrowth;
    }

    // Update is called once per frame
    void Update()
    {
        if (multiplier > 1f)
        multiplier -= Time.deltaTime * 0.05f;

        if (growthIndex < maxGrowth && !chopped)
        {  
            growthIndex += Time.deltaTime * 0.05f * maxGrowth * growthSpeed * multiplier;
            transform.localScale = new Vector3(1,1,1) * growthIndex;
        }
        else if (!isProduct && !reproductive)
        {
            StartCoroutine(GrowFruits());
            reproductive = true;
        }

        // shaking
        if (shakeAmplitude >= 0f)
        {
            shakeAmplitude -= 10f * Time.deltaTime;
            float rotX = Mathf.Sin(Time.time * 10f) * shakeAmplitude * shakeDirection.x;
            float rotZ = Mathf.Sin(Time.time * 10f) * shakeAmplitude * shakeDirection.y;
            transform.rotation = Quaternion.Euler(
                rotX, 
                transform.eulerAngles.y, 
                rotZ
            );
        }
    }

    public void Shake(float amplitude)
    {
        float myGrowth = transform.localScale.x / maxGrowth;
        GameObject burst = Instantiate(leaf, 
            transform.position + new Vector3(0f, 3f * myGrowth, 0f), 
            Quaternion.identity
        );
        burst.transform.localScale = new Vector3(myGrowth, myGrowth, myGrowth);
        
        shakeAmplitude = amplitude;
        shakeDirection = Random.insideUnitCircle.normalized;
    }

    public void Chop()
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;
        
        Vector3 randomDirection = new Vector3(
            Random.Range(-1f, 1f),
            0.5f,
            Random.Range(-1f, 1f)
        ).normalized;

        Vector3 forcePoint = rb.position + Vector3.up * (transform.localScale.y * 4f);

        rb.AddForceAtPosition(
            randomDirection * Random.Range(20f, 30f), 
            forcePoint, 
            ForceMode.Impulse
        );

        Vector3 randomTorque = new Vector3(
            Random.Range(-50f, 50f),
            Random.Range(-50f, 50f),
            Random.Range(-50f, 50f)
        );
        rb.AddTorque(randomTorque);
    }

    IEnumerator GrowFruits()
    {
        while (!chopped)
        {
            List<Transform> emptySlots = new List<Transform>();
            foreach (Transform slot in slots)
            {
                if (slot.childCount == 0) 
                {
                    emptySlots.Add(slot);
                }
            }

            if (emptySlots.Count == 0)
            {
                yield return new WaitForSeconds(Random.Range(3f, 6f) / multiplier);
                continue;
            }

            Transform toGrow = emptySlots[Random.Range(0,emptySlots.Count)];
            
            Instantiate(product, toGrow);
            fruitCount++;
            yield return new WaitForSeconds(Random.Range(3f, 6f) / multiplier);
        }
    }
}
