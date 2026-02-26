using UnityEngine;

public class HarvestTool : MonoBehaviour
{
    float maxDistance = 100f;
    [SerializeField] float radius, speed;
    [SerializeField] ParticleSystem mainVFX, subVFX;
        
    public void StartHarvest()
    {
        if (mainVFX) mainVFX.Play();
        if (subVFX) subVFX.Play();
    }

    public void StopHarvest()
    {
        if (mainVFX) mainVFX.Stop();
        if (subVFX) subVFX.Stop();
    }

    public void HarvestTree(GameObject ring, Ray ray, LayerMask gMask, LayerMask pMask)
    {
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, gMask))
        {
            if (hit.collider.CompareTag("Obstacle")) {
                StopHarvest();
                return;
            }

            ring.transform.localScale = new Vector3(0.2f * radius, 1f, 0.2f * radius);
            ring.transform.position = new Vector3(hit.point.x, 0.65f, hit.point.z);

            mainVFX.transform.position = hit.point;
            subVFX.transform.position = hit.point;

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
                if (tree != null) {
                    tree.harvestIndex += speed * Time.deltaTime;
                }
            }
        }
        else {
            Debug.Log("No hit detected");
        }
    }
}
