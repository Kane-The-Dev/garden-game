using UnityEngine;

public class HarvestTool : MonoBehaviour
{
    float maxDistance = 100f;
    [SerializeField] float radius;
        
    Inventory inventory;

    void Start() 
    {
        inventory = FindObjectOfType<Inventory>();
    }

    public void HarvestTree(Ray ray, LayerMask mask)
    {
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, mask))
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
    }
}
