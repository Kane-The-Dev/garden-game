using System.Linq;
using UnityEngine;

public class PlantTool : MonoBehaviour
{
    public int plantID;
    float maxDistance = 100f, radius = 0.5f;
    [SerializeField] GameObject[] plants;
    [SerializeField] float[] plantRadius;
    [SerializeField] GameObject[] products, other;
    Color currentColor;
    Renderer ringRender;
     
    Inventory inventory;

    void Start() 
    {
        currentColor = Color.white;
        inventory = GameManager.instance.inventory;
        plantID = -1;
    }

    int GetTreeType()
    {
        int treeType = 0;
        if (inventory.foodList[plantID].type == "Tree")
            treeType = Random.Range(0, 2);
        else if (inventory.foodList[plantID].type == "Pine")
            treeType = 2;
        else if (inventory.foodList[plantID].type == "Bush")
            treeType = Random.Range(3, 5);
        else if (inventory.foodList[plantID].type == "Ground")
            treeType = 5;
        else
            treeType = -1;

        return treeType;
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

            bool blocked = Physics.CheckSphere(
                hit.point,
                radius,
                oMask,
                QueryTriggerInteraction.Collide
            );

            Color targetColor = blocked
                ? new Color(1f, 0f, 0f, 0.8f)
                : new Color(1f, 1f, 1f, 0.8f);

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
            if (Physics.CheckSphere(hit.point, radius, oMask, QueryTriggerInteraction.Collide)) return;

            string plantName = inventory.foodList[plantID].name;
            if (inventory.myInventory[plantName] <= 0)
            {
                Debug.Log("Out of seed/item!");
                return;
            }

            if (plantID < 0) 
                return;
            else if (inventory.foodList[plantID].type != "Other") 
                Plant(plantID, hit.point);
            else 
                Build(plantID, hit.point);

            inventory.myInventory[plantName]--;
            inventory.exp += 25f;

            inventory.selection.RefreshPlants();
        }
    }

    void Plant(int plantID, Vector3 point)
    {
        int treeType = GetTreeType();
            
        GameObject newTree = Instantiate(
            plants[treeType], 
            point, 
            Quaternion.Euler(0f, Random.Range(0f, 180f), 0f)
        );

        var g = newTree.transform.GetChild(0).GetComponent<Growable>();
        g.growthSpeed = inventory.foodList[plantID].growthSpeed;
        g.maxGrowth *= Random.Range(0.85f, 1f);
        g.product = products[plantID];
        g.offset = Random.Range(0f, 90f);
        g.amplitude = Random.Range(3.6f, 4f) * (treeType > 2 ? 1f : 0.25f);
    }

    void Build(int plantID, Vector3 point)
    {
        Instantiate(
            other[plantID - products.Length],
            point, 
            Quaternion.Euler(0f, Random.Range(0f, 180f), 0f)
        );
    }
}
