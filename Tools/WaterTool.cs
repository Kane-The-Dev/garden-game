using UnityEngine;

public class WaterTool : MonoBehaviour
{
    float maxDistance = 100f;
    [SerializeField] ParticleSystem waterVFX;
    [SerializeField] float radius, multiplier;

    public void StartWater()
    {
        waterVFX.Play();
    }

    public void StopWater()
    {
        waterVFX.Stop();
    }

    public void WaterTree(Ray ray, LayerMask gMask, LayerMask fMask)
    {
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, gMask))
        {
            if (hit.collider.CompareTag("Obstacle")) {
                StopWater();
                return;
            }

            Vector3 pointA = hit.point + Vector3.up * 10f;
            Vector3 pointB = hit.point - Vector3.up * 5f;
            waterVFX.transform.position = hit.point + Vector3.up * 10f;

            Collider[] hits = Physics.OverlapCapsule(
                pointA,
                pointB,
                radius,
                fMask,
                QueryTriggerInteraction.Ignore
            );

            foreach (Collider p in hits)
            {
                Growable tree = p.GetComponent<Growable>();
                if (tree != null)
                {
                    tree.multiplier = multiplier;
                }
            }
        }
        else
        {
            Debug.Log("No hit detected");
        }
    }
}
