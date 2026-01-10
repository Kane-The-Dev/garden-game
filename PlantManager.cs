using UnityEngine;
using UnityEngine.EventSystems;

public class PlantManager : MonoBehaviour
{
    Camera cam;
    Inventory inventory;
    [SerializeField] GameObject[] plants, products, decorations;
    public float maxDistance = 100f;
    [SerializeField] LayerMask plantMask, groundMask, fruitMask;
    [SerializeField] ParticleSystem water;
    // [SerializeField] int[] priceList;
    public int mode; // 0 = plant, 1 = water, 2 = harvest, 3 = chop
    public int plantID;

    void Start()
    {
        cam = GetComponent<Camera>();
        inventory = FindObjectOfType<Inventory>();
        InitializeGarden();
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0) && mode == 1)
        {
            water.Stop();
        }

        if (Input.GetMouseButton(0) && mode == 1)
        {
            WaterTree();
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            return;

            if (mode == 0)
            PlantTree();
            else if (mode == 1)
            water.Play();
            else if (mode == 2)
            HarvestTree();
            else if (mode == 3)
            {
                HarvestTree();
                ChopTree();
            }
        }
    }

    void InitializeGarden()
    {
        int n = Random.Range(20, 30);
        float randomX, randomZ;

        for (int i = 0; i < n; i++)
        {
            randomX = Random.Range(-3f, 23f);
            randomZ = Random.Range(-3f, 23f);

            if (Physics.Raycast(new Vector3(randomX, 10f, randomZ), Vector3.down, out RaycastHit hit, maxDistance, groundMask))
                Instantiate(
                    decorations[Random.Range(0, decorations.Length)], 
                    hit.point, 
                    Quaternion.Euler(0f, Random.Range(-90f, 90f), 0f)
                );
        }
    }

    public void ChangeMode(int newMode)
    {
        mode = newMode;
    }

    public void ChangePlant(int newPlantID)
    {
        plantID = newPlantID;
    }

    void PlantTree()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, groundMask))
        {
            Growable[] trees = FindObjectsOfType<Growable>();
            foreach (Growable tree in trees)
            {
                if (!tree.isProduct && Vector3.Distance(tree.transform.position, hit.point) < 3.5f)
                {
                    Debug.Log("Overlap other trees");
                    return;
                }
            }

            if (inventory.coin < inventory.foodList[plantID].plantPrice)
            {
                Debug.Log("No more money");
                return;
            }

            int random = Random.Range(0, plants.Length);
            GameObject newTree = Instantiate(
                plants[random], 
                hit.point, 
                Quaternion.Euler(0f, Random.Range(0f, 180f), 0f)
            );

            var g = newTree.GetComponent<Growable>();
            g.growthSpeed = inventory.foodList[plantID].growthSpeed;
            g.maxGrowth *= Random.Range(0.85f, 1f);
            g.product = products[plantID];
            
            inventory.coin -= inventory.foodList[plantID].plantPrice;
            inventory.exp += 25f;
        }
        else
        {
            Debug.Log("No hit detected");
        }
    }

    void WaterTree()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, groundMask))
        {
            water.transform.position = hit.point + Vector3.up * 8f;
            RaycastHit[] hits = Physics.SphereCastAll(
                hit.point + Vector3.up * 10f,
                2f,
                Vector3.down,
                maxDistance,
                fruitMask
            );

            foreach (RaycastHit p in hits)
            {
                Growable tree = p.collider.GetComponent<Growable>();
                if (tree != null)
                {
                    tree.multiplier = 1.5f;
                }
            }
        }
        else
        {
            Debug.Log("No hit detected");
        }
    }

    void HarvestTree()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, plantMask))
        {
            Growable thisTree = hit.collider.gameObject.GetComponent<Growable>();
            if (!thisTree) 
            {
                Debug.Log("no tree?");
                return;
            }
            
            thisTree.Shake(5f);
            foreach (Transform slot in thisTree.slots)
            {
                if (slot.childCount > 0 && slot.GetChild(0).GetComponent<Growable>())
                {
                    Growable thisFruit = slot.GetChild(0).GetComponent<Growable>();
                    if (thisFruit.growthIndex >= 0.9 * thisFruit.maxGrowth)
                    {
                        slot.GetChild(0).parent = null;

                        var rb = thisFruit.gameObject.GetComponent<Rigidbody>();
                        rb.constraints = RigidbodyConstraints.None;
                        rb.useGravity = true;

                        inventory.exp += 3f;
                        inventory.foodList[thisFruit.productID].UpdateN(1);

                        thisTree.fruitCount--;

                        Destroy(thisFruit.gameObject, 3f);
                    }
                    // else Debug.Log("Nothing to harvest!");
                }
            }

            inventory.UpdateStorage();
        }
        else
        {
            Debug.Log("No hit detected");
        }
    }

    void ChopTree()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, plantMask))
        {
            Growable thisTree = hit.collider.gameObject.GetComponent<Growable>();
            if (!thisTree) return;

            thisTree.Chop();
            thisTree.chopped = true;

            GetComponent<Inventory>().exp += 10f;
            Destroy(thisTree.gameObject, 5f);
        }
        else
        {
            Debug.Log("No hit detected");
        }
    }
}
