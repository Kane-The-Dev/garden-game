using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Growable : MonoBehaviour
{
    [Header("Growth")]
    public float maxGrowth;
    public float growthIndex, growthSpeed = 1f, multiplier, subMultiplier;
    public float timeIndex;

    [Header("Tree")]
    public GameObject product;
    public GameObject leaf;
    public Transform[] slots;
    public bool reproductive = false;
    public int fruitCount;
    public float harvestIndex, chopIndex;
    int chopStage = 0;
    const float STAGE1 = 0.3333f;
    const float STAGE2 = 0.6666f;
    
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
        subMultiplier = 1f;
        transform.localScale = Vector3.one * 0.2f * maxGrowth;
        harvestIndex = 0f;
        chopIndex = 0f;
    }

    void Update()
    {
        timeIndex = GameManager.instance.timeControl * 0.05f;

        Growing();
        Shaking();
        Harvesting();
        Chopping();
    }

    void Growing()
    {
        if (multiplier > 1f)
        multiplier -= Time.deltaTime * timeIndex;

        if (subMultiplier > 1f)
        subMultiplier -= Time.deltaTime * timeIndex;

        if (growthIndex < maxGrowth && !chopped)
        {  
            growthIndex += Time.deltaTime * timeIndex * maxGrowth * growthSpeed * multiplier * subMultiplier;
            transform.localScale = Vector3.one * growthIndex;
        }
        else if (!isProduct && !reproductive)
        {
            StartCoroutine(GrowFruits());
            reproductive = true;
        }
    }

    void Shaking()
    {
        if (!isProduct && shakeAmplitude >= 0f)
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

    void Harvesting()
    {
        if (harvestIndex > 0f)
            harvestIndex -= Time.deltaTime / 3f;
        
        if (harvestIndex >= 1f)
        {
            harvestIndex = 0f;
            HarvestFruit(2);
            Shake(5f);
        }
    }

    void Chopping()
    {
        if (chopped) return;

        if (chopIndex > 0f)
            chopIndex -= Time.deltaTime / 3f;

        chopIndex = Mathf.Clamp01(chopIndex);

        // Determine stage from chopIndex
        int newStage;
        if (chopIndex >= 1f)
            newStage = 3;
        else if (chopIndex >= STAGE2)
            newStage = 2;
        else if (chopIndex >= STAGE1)
            newStage = 1;
        else
            newStage = 0;

        if (newStage > chopStage)
        {
            // Stage 0 → 1
            if (chopStage < 1 && newStage >= 1)
            {
                HarvestFruit(2);
                Shake(5f);
            }

            // Stage 1 → 2
            if (chopStage < 2 && newStage >= 2)
            {
                HarvestFruit(2);
                Shake(5f);
            }

            // Stage 2 → 3 (final)
            if (newStage == 3)
            {
                HarvestFruit(10);
                Chop();
                Destroy(gameObject, 5f);
            }
        }

        chopStage = newStage;
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

    public void HarvestFruit(int quantity)
    {
        Debug.Log("Harvesting " + quantity + " fruits!");
        if (quantity <= 0) return;

        foreach (Transform slot in slots)
        {
            if (slot.childCount > 0)
            {
                Growable thisFruit = slot.GetChild(0).GetComponent<Growable>();
                if (thisFruit && thisFruit.growthIndex >= 0.9 * thisFruit.maxGrowth)
                {
                    slot.GetChild(0).parent = null;

                    var rb = thisFruit.gameObject.GetComponent<Rigidbody>();
                    rb.constraints = RigidbodyConstraints.None;
                    rb.useGravity = true;
                    
                    Inventory inventory = GameManager.instance.inventory;
                    inventory.exp += 3f;
                    inventory.foodList[thisFruit.productID].UpdateN(1);
                    inventory.UpdateStorage();

                    fruitCount--;
                    Destroy(thisFruit.gameObject, 3f);
                    
                    if (--quantity <= 0) break;
                }
            }
        }
    }

    public void Chop()
    {
        if (chopped) return;

        chopped = true;
        GameManager.instance.inventory.exp += 10f;

        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;
        
        Vector3 randomDirection = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;

        Vector3 forcePoint = rb.position + Vector3.up * (transform.localScale.y * 4f);

        rb.AddForceAtPosition(
            randomDirection * Random.Range(5f, 10f) * transform.localScale.x, 
            forcePoint, 
            ForceMode.Impulse
        );

        Vector3 randomTorque = new Vector3(
            Random.Range(-20f, 20f),
            Random.Range(-20f, 20f),
            Random.Range(-20f, 20f)
        );
        rb.AddTorque(randomTorque);
    }

    IEnumerator GrowFruits()
    {
        while (!chopped)
        {
            while (timeIndex <= 0.005f)
                yield return null;

            List<Transform> emptySlots = new List<Transform>();
            foreach (Transform slot in slots)
            {
                if (slot.childCount == 0) 
                {
                    emptySlots.Add(slot);
                }
            }

            float delay = Random.Range(0.15f, 0.3f) / (growthSpeed * multiplier) / timeIndex;

            if (emptySlots.Count == 0)
            {
                yield return new WaitForSeconds(delay);
                continue;
            }

            Transform toGrow = emptySlots[Random.Range(0, emptySlots.Count)];
            
            Instantiate(product, toGrow);

            fruitCount++;
            yield return new WaitForSeconds(delay);
        }
    }
}
