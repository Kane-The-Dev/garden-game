using System.Collections.Generic;
using UnityEngine;

public class PlantTool : MonoBehaviour
{
    public int plantID;
    float maxDistance = 100f, radius = 0.5f;
    [SerializeField] GameObject[] plants;
    [SerializeField] float[] plantRadius;
    Color currentColor;
    [SerializeField] Color valid, notValid;
    [SerializeField] Material gold;
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

    int GetTreeType(int ID = -1)
    {
        if (ID < 0) ID = plantID;

        string type = inventory.foodList[ID].type;
        if (type == "Tree")
            return Random.Range(0, 2);
        if (type == "Pine")   
            return 2;
        if (type == "Bush")   
            return Random.Range(3, 5);
        if (type == "Ground") 
            return 5;
        if (type == "Oven")   
            return 6;
        return -1;
    }

    bool IsOven(int ID = -1) { if (ID < 0) ID = plantID; return ID >= 0 && inventory.foodList[ID].type == "Oven"; }

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

        if (IsOven()) Plant(plantID, hit.point, Quaternion.Euler(0f, Random.Range(0f, 180f), 0f), validOven);
        else Plant(plantID, hit.point, Quaternion.Euler(0f, Random.Range(0f, 180f), 0f));

        inventory.AddItemQuantity(plantName, -1);
        inventory.exp += 20f;
        inventory.selection.RefreshPlants();
    }

    public Growable Plant(int ID, Vector3 position, Quaternion rotation, Transform parent = null, int treeType = -1)
    {
        if (treeType < 0) treeType = GetTreeType(ID);

        GameObject newTree = parent
            ? Instantiate(plants[treeType], parent)
            : Instantiate(plants[treeType], position, rotation);

        Growable g = newTree.GetComponentInChildren<Growable>();
        if (!g) return null;

        if (parent) parent.GetComponentInChildren<FollowTransform>().target = g.transform;

        Item item = inventory.foodList[ID];
        g.growthSpeed.baseValue = item.growthSpeed;
        g.productID = ID;
        g.isOven = IsOven(ID);
        g.wiggleOffset = Random.Range(0f, 90f);
        g.wiggleAmplitude *= Random.Range(4f, 5f);

        ResearchCenter research = GameManager.instance.research;
        foreach (Modifier mod in research.generalGrowthMods)
            g.growthSpeed.AddModifier(mod);

        List<Modifier> specificMods = g.isOven ? research.ovenGrowthMods : research.plantGrowthMods;
        foreach (Modifier mod in specificMods)
            g.growthSpeed.AddModifier(mod);  

        if (!g.isOven) {
            g.maxGrowth *= Random.Range(0.85f, 1f);

            foreach (Modifier mod in research.fruitCountMods)
                g.blockedSlotCount.AddModifier(mod);
        }
        else g.blockedSlotCount.baseValue = 0;

        GameObject productPrefab = inventory.LoadProductPrefab(item.name);
        if (productPrefab != null) g.product = productPrefab;

        return g;
    }
}