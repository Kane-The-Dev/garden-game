using UnityEngine;

public class HarvestTool : MonoBehaviour
{
    float maxDistance = 100f;
    [SerializeField] float radius, speed;
        
    Inventory inventory;

    void Start() 
    {
        inventory = GameManager.instance.inventory;
    }

    public void HarvestTree(Ray ray, LayerMask gMask, LayerMask pMask)
    {
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, gMask))
        {
            if (hit.collider.CompareTag("Obstacle")) {
                return;
            }

            Vector3 pointA = hit.point + Vector3.up * 10f;
            Vector3 pointB = hit.point - Vector3.up * 5f;

            Collider[] hits = Physics.OverlapCapsule(
                pointA,
                pointB,
                radius,
                pMask,
                QueryTriggerInteraction.Ignore
            );

            foreach (Collider p in hits)
            {
                Growable tree = p.GetComponent<Growable>();
                if (tree != null)
                {
                    tree.harvestIndex += speed * Time.deltaTime;
                }
            }
        }
        else
        {
            Debug.Log("No hit detected");
        }
    }
}
