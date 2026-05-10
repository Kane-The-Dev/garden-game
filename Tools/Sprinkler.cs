using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sprinkler : MonoBehaviour
{
    [SerializeField] float radius, bonus;
    [SerializeField] LayerMask fMask;
    [SerializeField] ParticleSystem[] waterHoses;

    void Start()
    {
        StartCoroutine(Sprinkle());
    }

    IEnumerator Sprinkle()
    {
        yield return new WaitForSeconds(0.1f);
        if (GetComponent<Constructible>().isPreview) yield break;

        GetComponent<Spin>().speed = 90;
        foreach (ParticleSystem water in waterHoses)
            water.Play();
        
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
                    tree.subMultiplier = 1 + bonus;
                }
            }
        }
    }
}
