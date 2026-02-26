using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sprinkler : MonoBehaviour
{
    [SerializeField] float radius, bonus;
    [SerializeField] LayerMask fMask;

    void Start()
    {
        StartCoroutine(Sprinkle());
    }

    IEnumerator Sprinkle()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            Vector3 pointA = transform.position + Vector3.up * 10f;
            Vector3 pointB = transform.position - Vector3.up * 5f;

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
                if (tree != null) {
                    // Debug.Log("Nearby tree found");
                    tree.subMultiplier = 1 + bonus;
                }
            }
        }
        
    }
}
