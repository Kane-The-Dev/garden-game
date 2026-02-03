using UnityEngine;

public class PlantTool : MonoBehaviour
{
    float maxDistance = 100f;
    [SerializeField] GameObject[] plants, products;
     
    Inventory inventory;

    void Start() 
    {
        inventory = FindObjectOfType<Inventory>();
    }

    public void PlantTree(int plantID, Ray ray, LayerMask mask)
    {
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, mask))
        {
            if (hit.collider.CompareTag("Obstacle")) {
                Debug.Log("Obstacle hit");
                return;
            }

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
    }
}
