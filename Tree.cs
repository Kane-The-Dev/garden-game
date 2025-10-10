using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    [Header("Growth")]
    public float maxGrowth;
    public float growthIndex, growthSpeed = 1f, multiplier;

    [Header("Tree")]
    public GameObject product;
    public Transform[] slots;
    public bool reproductive = false;
    public int fruitCount;
    
    [Header("Product")]
    public bool isProduct;
    public int productID;

    [Header("Other")]
    public bool chopped = false;
    public float shakeAmplitude;

    void Start()
    {
        growthIndex = 0.2f;
        multiplier = 1f;
        transform.localScale = new Vector3(1,1,1) * growthIndex;
    }

    // Update is called once per frame
    void Update()
    {
        if (multiplier > 1f)
        multiplier -= Time.deltaTime * 0.05f;

        if (growthIndex < maxGrowth && !chopped)
        {  
            growthIndex += Time.deltaTime * 0.05f * maxGrowth * growthSpeed * multiplier * 10f;
            transform.localScale = new Vector3(1,1,1) * growthIndex;
        }
        else if (!isProduct && !reproductive)
        {
            StartCoroutine(GrowFruits());
            reproductive = true;
        }

        if (shakeAmplitude >= 0f)
        {
            shakeAmplitude -= 10f * Time.deltaTime;
            float rotX = Mathf.Sin(Time.time * 10f) * shakeAmplitude;
            transform.rotation = Quaternion.Euler(rotX, transform.eulerAngles.y, transform.eulerAngles.z);
        }
    }

    public void Shake(float amplitude)
    {
        shakeAmplitude = amplitude;
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
                yield return new WaitForSeconds(Random.Range(3f,6f));
                continue;
            }

            Transform toGrow = emptySlots[Random.Range(0,emptySlots.Count)];
            
            Instantiate(product, toGrow);
            fruitCount++;
            yield return new WaitForSeconds(Random.Range(3f,6f));
        }
    }
}
