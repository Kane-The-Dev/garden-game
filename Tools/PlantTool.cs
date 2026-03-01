using UnityEngine;

public class PlantTool : MonoBehaviour
{
    float maxDistance = 100f;
    [SerializeField] GameObject[] plants, products, other;
     
    Inventory inventory;

    void Start() 
    {
        inventory = GameManager.instance.inventory;
    }

    public void PlantTree(int plantID, Ray ray, LayerMask mask)
    {
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, mask))
        {
            if (hit.collider.CompareTag("Obstacle")) {
                Debug.Log("Hit an obstacle!");
                return;
            }

            string plantName = inventory.foodList[plantID].name;
            if (inventory.myInventory[plantName] <= 0)
            {
                Debug.Log("Out of seed/item!");
                return;
            }

            foreach (var tree in FindObjectsOfType<Growable>())
            {
                if (!tree.isProduct && Vector3.Distance(tree.transform.position, hit.point) < 3.5f)
                {
                    Debug.Log("Overlap other trees!");
                    return;
                }
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
        int treeType = 0;
        if (inventory.foodList[plantID].type == "Tree")
            treeType = Random.Range(0, 2);
        else if (inventory.foodList[plantID].type == "Pine")
            treeType = 2;
        else if (inventory.foodList[plantID].type == "Bush")
            treeType = Random.Range(3, 5);
        else if (inventory.foodList[plantID].type == "Ground")
            treeType = 5;
            
        GameObject newTree = Instantiate(
            plants[treeType], 
            point, 
            Quaternion.Euler(0f, Random.Range(0f, 180f), 0f)
        );

        var g = newTree.GetComponent<Growable>();
        g.growthSpeed = inventory.foodList[plantID].growthSpeed;
        g.maxGrowth *= Random.Range(0.85f, 1f);
        g.product = products[plantID];
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
