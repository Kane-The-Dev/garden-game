using UnityEngine;

public class HarvestTool : MonoBehaviour
{
    float maxDistance = 100f;
    [SerializeField] float radius, speed;
    [SerializeField] ParticleSystem mainVFX, subVFX;
    [SerializeField] AdvancedAudioSource myAAS;
    [SerializeField] Tornado myWind;
    [SerializeField] bool screenShake = false;

    GameManager gm;

    void Start()
    {
        gm = GameManager.instance;
    }
        
    public void StartHarvest()
    {
        if (mainVFX) mainVFX.Play();
        if (subVFX) subVFX.Play();
        if (myAAS) myAAS.Play(null, -1f, false, 0.5f);
        if (myWind) myWind.StartWind();
        if (screenShake) gm.cam.StartScreenShake(0.02f);
    }

    public void StopHarvest()
    {
        if (mainVFX) mainVFX.Stop();
        if (subVFX) subVFX.Stop();
        if (myAAS) myAAS.Stop(0.5f);
        if (myWind) myWind.StopWind();
        if (screenShake) gm.cam.StopScreenShake();
    }

    public void HarvestTree(GameObject ring, Ray ray, LayerMask gMask, LayerMask pMask, float multiplier)
    {
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, gMask))
        {
            if (hit.collider.CompareTag("Obstacle")) {
                StopHarvest();
                return;
            }

            ring.transform.localScale = new Vector3(0.2f * radius, 1f, 0.2f * radius);
            ring.transform.position = new Vector3(hit.point.x, hit.point.y + 0.1f, hit.point.z);

            if (mainVFX) mainVFX.transform.position = hit.point;
            if (subVFX) subVFX.transform.position = hit.point;

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
                    tree.harvestIndex += speed * multiplier * Time.deltaTime;
                }
            }
        }
        else {
            Debug.Log("No hit detected");
        }
    }
}
