using System.Collections;
using UnityEngine;

public class ChopTool : MonoBehaviour
{
    float maxDistance = 100f, radius = 1f;
    [SerializeField] float speed;
    [SerializeField] AudioClip[] sounds;
    public AdvancedAudioSource myAAS;

    Inventory inventory;

    void Start() 
    {
        inventory = GameManager.instance.inventory;
    }

    public void PlayChop()
    {
        if (myAAS) myAAS.PlayOneShot(sounds[Random.Range(0, sounds.Length)], -1f, true);
    }

    public void ChopTree(GameObject ring, Ray ray, LayerMask gMask, LayerMask pMask)
    {
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, gMask))
        {
            if (hit.collider.CompareTag("Obstacle")) {
                return;
            }

            ring.transform.localScale = new Vector3(0.2f * radius, 1f, 0.2f * radius);
            ring.transform.position = new Vector3(hit.point.x, hit.point.y + 0.1f, hit.point.z);

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

                if (tree.myChopTool != this)
                    tree.myChopTool = this;

                if (tree != null && !tree.chopped)
                    tree.chopIndex += speed * Time.deltaTime;
            }
        }
    }
}
