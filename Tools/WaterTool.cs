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

            waterVFX.transform.position = hit.point + Vector3.up * 8f;

            Collider[] hits = Physics.OverlapCapsule(
                hit.point + Vector3.up * 20f,
                hit.point,
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
