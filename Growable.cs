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
    [SerializeField] Transform effectSpawnPoint;
    int chopStage = 0;
    public float STAGE1 = 0.3333f;
    public float STAGE2 = 0.6666f;
    public float STAGE3 = 1f;
    
    [Header("Product")]
    public bool isProduct;
    public int productID;
    Collider col;

    [Header("Wiggle")]
    public float offset;
    public float amplitude;

    [Header("Sound Effect")]
    [SerializeField] AudioSource source;
    [SerializeField] AudioClip[] plant, chop, leaves, fall;

    [Header("Other")]
    public bool chopped = false;
    [SerializeField] float shakeAmplitude;
    Vector2 shakeDirection;
    GameManager gm;

    void Awake()
    {
        growthIndex = 0.2f;
        multiplier = 1f;
        subMultiplier = 1f;
        transform.localScale = Vector3.one * 0.2f * maxGrowth;
        harvestIndex = 0f;
        chopIndex = 0f;

        if (!isProduct) {
            RandomizeAudio();
            source.PlayOneShot(plant[Random.Range(0, plant.Length)]);
        }
    }

    void Start()
    {
        gm = GameManager.instance;
        if (isProduct && growthSpeed != 0) {
            col = GetComponent<Collider>();
            col.isTrigger = true;
        }
    }

    public IEnumerator ActivateCollider()
    {
        yield return new WaitForSeconds(0.3f);
        col.isTrigger = false;
    }

    void Update()
    {
        timeIndex = gm.timeControl * 0.05f;

        Growing();

        if (isProduct) return;

        Wiggling();
        Shaking();
        Harvesting();
        Chopping();
    }

    void Wiggling()
    {
        float rotX = Mathf.Sin(Time.time * 0.5f + offset) * amplitude;
        float rotZ = Mathf.Sin(Time.time * 0.5f + offset) * amplitude;

        Vector3 euler = transform.parent.eulerAngles;
        transform.parent.rotation = Quaternion.Euler(rotX, euler.y, rotZ);
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

    public void Shake(float amplitude)
    {
        if (isProduct) return;

        RandomizeAudio();
        source.PlayOneShot(leaves[Random.Range(0, leaves.Length)]);

        float myGrowth = transform.localScale.x / maxGrowth;
        GameObject burst = Instantiate(leaf, 
            effectSpawnPoint.position, 
            Quaternion.identity
        );
        burst.transform.localScale = new Vector3(myGrowth, myGrowth, myGrowth);
        
        shakeAmplitude = amplitude;
        shakeDirection = Random.insideUnitCircle.normalized;
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
            Shake(3f);
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
        if (chopIndex >= STAGE3)
            newStage = 3;
        else if (chopIndex >= STAGE2)
            newStage = 2;
        else if (chopIndex >= STAGE1)
            newStage = 1;
        else
            newStage = 0;

        if (newStage > chopStage)
        {
            gm.cam.ScreenShake(0.05f);
            RandomizeAudio();
            source.PlayOneShot(chop[Random.Range(0, chop.Length)]);

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
                Destroy(transform.parent.gameObject, 5f);

                RandomizeAudio();
                source.PlayOneShot(fall[0]);
            }
        }

        chopStage = newStage;
    }

    public void HarvestFruit(int quantity)
    {
        // Debug.Log("Harvesting " + quantity + " fruits!");
        if (quantity <= 0) return;

        foreach (Transform slot in slots)
        {
            if (slot.childCount > 0)
            {
                Growable thisFruit = slot.GetChild(0).GetComponent<Growable>();
                if (thisFruit && thisFruit.growthIndex >= 0.9 * thisFruit.maxGrowth)
                {
                    thisFruit.chopped = true;
                    thisFruit.StartCoroutine(thisFruit.ActivateCollider());
                    slot.GetChild(0).parent = null;

                    var rb = thisFruit.gameObject.GetComponent<Rigidbody>();
                    rb.constraints = RigidbodyConstraints.None;
                    rb.useGravity = true;

                    Inventory inventory = GameManager.instance.inventory;

                    Vector3 dir = (thisFruit.transform.position - transform.position).normalized;
                    // Debug.Log(inventory.foodList[thisFruit.productID].weight);
                    rb.AddForce(
                        new Vector3(dir.x, 6f, dir.z) * inventory.foodList[thisFruit.productID].weight, 
                        ForceMode.Impulse
                    );
                    
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

        foreach (Transform slot in slots)
        {
            if (slot.childCount > 0)
                slot.GetChild(0).GetComponent<Growable>().chopped = true;
        }

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
            
            var newProduct = Instantiate(product, toGrow);
            var newFruit = newProduct.GetComponent<Growable>();
            newFruit.growthSpeed = growthSpeed;

            fruitCount++;
            yield return new WaitForSeconds(delay);
        }
    }

    void RandomizeAudio()
    {
        source.pitch = Random.Range(0.9f, 1.1f);
        source.volume = Random.Range(0.85f, 1f);
    }
}
