using UnityEngine;

public class PlantTool : MonoBehaviour
{
    public int plantID;
    float maxDistance = 100f, radius = 0.5f;
    [SerializeField] GameObject[] plants;
    [SerializeField] string productsFolderPath = "Products";
    [SerializeField] float[] plantRadius;
    Color currentColor;
    [SerializeField] Color valid, notValid;
    Renderer ringRender;
    Collider[] overlapResults = new Collider[16];
    Transform validOven;
     
    Inventory inventory;

    void Start() 
    {
        currentColor = valid;
        inventory = GameManager.instance.inventory;
        plantID = -1;
    }

    int GetTreeType()
    {
        int treeType;
        if (inventory.foodList[plantID].type == "Tree")
            treeType = Random.Range(0, 2);
        else if (inventory.foodList[plantID].type == "Pine")
            treeType = 2;
        else if (inventory.foodList[plantID].type == "Bush")
            treeType = Random.Range(3, 5);
        else if (inventory.foodList[plantID].type == "Ground")
            treeType = 5;
        else if (inventory.foodList[plantID].type == "Oven")
            treeType = 6;
        else
            treeType = -1;

        return treeType;
    }

    bool IsOven()
    {
        return plantID >= 0 && inventory.foodList[plantID].type == "Oven";
    }

    bool IsBlocked(Vector3 point, LayerMask oMask) 
    {
        validOven = null;

        bool blocked = Physics.CheckSphere(
            point,
            radius,
            oMask,
            QueryTriggerInteraction.Collide
        );

        if (IsOven())
        {
            blocked = true;

            int hitCount = Physics.OverlapSphereNonAlloc(
                point,
                radius,
                overlapResults,
                oMask,
                QueryTriggerInteraction.Collide
            );

            for (int i = 0; i < hitCount; i++)
            {
                Collider other = overlapResults[i];

                if (!other)
                    continue;

                if (
                    other.CompareTag("Oven") 
                    && !other.transform.parent.GetComponentInChildren<Growable>()
                ) {
                    blocked = false;
                    validOven = other.transform.parent; // get root of oven
                    break;
                }
            }
        }

        return blocked;
    }

    public void PlantCheck(GameObject ring, Ray ray, LayerMask gMask, LayerMask oMask)
    {
        if (plantID < 0) return;

        int treeType = GetTreeType();
        radius = treeType >= 0 ? plantRadius[treeType] : 0.5f;
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, gMask))
        {
            ring.transform.localScale = new Vector3(0.2f * radius, 1f, 0.2f * radius);
            ring.transform.position = new Vector3(hit.point.x, hit.point.y + 0.1f, hit.point.z);

            Color targetColor = IsBlocked(hit.point, oMask) ? notValid : valid;

            // Smooth transition
            currentColor = Color.Lerp(
                currentColor,
                targetColor,
                Time.deltaTime * 20f
            );
            
            if (!ringRender) ringRender = ring.GetComponent<Renderer>();
            ringRender.material.color = currentColor;
        }
    }

    public void PlantTree(Ray ray, LayerMask gMask, LayerMask oMask)
    {
        if (plantID < 0) return;

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, gMask))
        {
            if (IsBlocked(hit.point, oMask)) return;

            string plantName = inventory.foodList[plantID].name;
            if (inventory.myInventory[plantName] <= 0)
            {
                Debug.Log("Out of seed/item!");
                return;
            }

            if (plantID < 0) return;
            else if (IsOven()) Plant(hit.point, validOven);
            else Plant(hit.point);

            inventory.myInventory[plantName]--;
            inventory.exp += 25f;

            inventory.selection.RefreshPlants();
        }
    }

    void Plant(Vector3 point, Transform parent = null)
    {
        int treeType = GetTreeType();
            
        GameObject newTree;
        Growable g;

        if (!parent)
        {
            newTree = Instantiate(
                plants[treeType], 
                point, 
                Quaternion.Euler(0f, Random.Range(0f, 180f), 0f)
            );
            g = newTree.GetComponentInChildren<Growable>();
        }
        else
        {
            newTree = Instantiate(
                plants[treeType], 
                parent
            );
            g = newTree.GetComponentInChildren<Growable>();
            parent.GetComponentInChildren<FollowTransform>().target = g.transform;
        }

        if (!g) return;

        g.growthSpeed = inventory.foodList[plantID].growthSpeed;
        if (!IsOven()) g.maxGrowth *= Random.Range(0.85f, 1f);
        else g.isOven = true;

        GameObject productPrefab = LoadProductPrefab(inventory.foodList[plantID].name);
        if (productPrefab != null)
        {
            g.product = productPrefab;
        }
        else
        {
            Debug.LogWarning($"No product prefab found for '{inventory.foodList[plantID].name}' in Resources/{productsFolderPath}");
        }

        g.wiggleOffset = Random.Range(0f, 90f);
        g.wiggleAmplitude *= Random.Range(4f, 5f);
    }

    GameObject LoadProductPrefab(string itemName)
    {
        if (string.IsNullOrEmpty(itemName))
            return null;

        string resourcePath = string.IsNullOrEmpty(productsFolderPath)
            ? itemName
            : productsFolderPath + "/" + itemName;

        GameObject prefab = Resources.Load<GameObject>(resourcePath);

        if (prefab == null)
        {
            string fallbackPath = string.IsNullOrEmpty(productsFolderPath)
                ? itemName.Replace(" ", string.Empty)
                : productsFolderPath + "/" + itemName.Replace(" ", string.Empty);

            prefab = Resources.Load<GameObject>(fallbackPath);
        }

        return prefab;
    }
}
