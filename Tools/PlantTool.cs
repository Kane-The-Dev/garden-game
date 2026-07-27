using UnityEngine;

public class PlantTool : MonoBehaviour
{
    public int plantID;
    float maxDistance = 100f, radius = 0.5f;
    [SerializeField] GameObject[] plants;
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
        string type = inventory.foodList[plantID].type;
        if (type == "Tree")   return Random.Range(0, 2);
        if (type == "Pine")   return 2;
        if (type == "Bush")   return Random.Range(3, 5);
        if (type == "Ground") return 5;
        if (type == "Oven")   return 6;
        return -1;
    }

    bool IsOven() => plantID >= 0 && inventory.foodList[plantID].type == "Oven";

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
                if (!other) continue;

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

        if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, gMask)) return;

        ring.transform.localScale = new Vector3(0.2f * radius, 1f, 0.2f * radius);
        ring.transform.position = new Vector3(hit.point.x, hit.point.y + 0.1f, hit.point.z);

        Color targetColor = IsBlocked(hit.point, oMask) ? notValid : valid;
        currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * 20f);

        if (!ringRender) ringRender = ring.GetComponent<Renderer>();
        ringRender.material.color = currentColor;
    }

    public void PlantTree(Ray ray, LayerMask gMask, LayerMask oMask)
    {
        if (plantID < 0) return;

        if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, gMask)) return;
        if (IsBlocked(hit.point, oMask)) return;

        string plantName = inventory.foodList[plantID].name;
        if (inventory.GetQuantity(plantName) <= 0)
        {
            Debug.Log("Out of seed/item!");
            return;
        }

        if (IsOven()) Plant(hit.point, validOven);
        else Plant(hit.point);

        inventory.AddItemQuantity(plantName, -1);
        inventory.exp += 25f;
        inventory.selection.RefreshPlants();
    }

    void Plant(Vector3 point, Transform parent = null)
    {
        int treeType = GetTreeType();

        GameObject newTree = parent
            ? Instantiate(plants[treeType], parent)
            : Instantiate(plants[treeType], point, Quaternion.Euler(0f, Random.Range(0f, 180f), 0f));

        Growable g = newTree.GetComponentInChildren<Growable>();
        if (!g) return;

        if (parent) parent.GetComponentInChildren<FollowTransform>().target = g.transform;

        Item item = inventory.foodList[plantID];
        g.growthSpeed = item.growthSpeed;
        g.productID = plantID;
        g.isOven = IsOven();
        if (!g.isOven) g.maxGrowth *= Random.Range(0.85f, 1f);

        GameObject productPrefab = inventory.LoadProductPrefab(item.name);
        if (productPrefab != null) g.product = productPrefab;

        g.wiggleOffset = Random.Range(0f, 90f);
        g.wiggleAmplitude *= Random.Range(4f, 5f);
    }
}