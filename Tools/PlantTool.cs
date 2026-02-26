using UnityEngine;

public class PlantTool : MonoBehaviour
{
    float maxDistance = 100f;
    [SerializeField] GameObject[] plants, products;
     
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

            if (plantID < 0) return;

            string plantName = inventory.foodList[plantID].name;
            if (inventory.myInventory[plantName] <= 0)
            {
                Debug.Log("Out of seed!");
                return;
            }

            Growable[] trees = FindObjectsOfType<Growable>();
            foreach (Growable tree in trees)
            {
                if (!tree.isProduct && Vector3.Distance(tree.transform.position, hit.point) < 3.5f)
                {
                    Debug.Log("Overlap other trees!");
                    return;
                }
            }

            int random = 0;
            if (inventory.foodList[plantID].type == "Tree")
                random = Random.Range(0, 2);
            else if (inventory.foodList[plantID].type == "Pine")
                random = 2;
            else if (inventory.foodList[plantID].type == "Bush")
                random = Random.Range(3, 5);
            else if (inventory.foodList[plantID].type == "Ground")
                random = 5;
                
            GameObject newTree = Instantiate(
                plants[random], 
                hit.point, 
                Quaternion.Euler(0f, Random.Range(0f, 180f), 0f)
            );

            var g = newTree.GetComponent<Growable>();
            g.growthSpeed = inventory.foodList[plantID].growthSpeed;
            g.maxGrowth *= Random.Range(0.85f, 1f);
            g.product = products[plantID];
            
            inventory.myInventory[plantName]--;
            inventory.exp += 25f;

            inventory.selection.RefreshPlants();
        }
    }
}
