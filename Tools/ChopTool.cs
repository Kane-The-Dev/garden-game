using UnityEngine;

public class ChopTool : MonoBehaviour
{
    float maxDistance = 100f;
    [SerializeField] float delay;

    Inventory inventory;

    void Start() 
    {
        inventory = FindObjectOfType<Inventory>();
    }

    public void ChopTree(Ray ray, LayerMask mask)
    {
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, mask))
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
