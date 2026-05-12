using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoHarvester : MonoBehaviour
{
    [SerializeField] float scanRadius = 15f, speed = 5f, scanCooldown = 5f, harvestCooldown = 2f;
    [SerializeField] LayerMask plantMask;
    [SerializeField] Transform helper;
    float timer = 0f;
    Collider[] overlapResults = new Collider[16];
    [SerializeField] Growable target;

    void Start()
    {
        StartCoroutine(ScanPlants());
    }

    void Update()
    {
        if (target)
        {
            if (Vector3.Distance(helper.position, target.transform.position) > 2f)
                helper.position = Vector3.MoveTowards(
                    helper.position, 
                    target.transform.position, 
                    speed * Time.deltaTime
                );
            else if (timer <= 0f)
            {
                target.HarvestFruit(2);
                target.Shake(4f);
                timer = harvestCooldown;
                if (target.ripeFruitCount <= 0) target = null;
            }
        }

        if (timer > 0f) timer -= Time.deltaTime;
    }

    IEnumerator ScanPlants()
    {
        while (true)
        {
            yield return new WaitForSeconds(scanCooldown);

            int hitCount = Physics.OverlapSphereNonAlloc(
                transform.position,
                scanRadius,
                overlapResults,
                plantMask,
                QueryTriggerInteraction.Collide
            );

            List<Growable> candidates = new List<Growable>();

            for (int i = 0; i < hitCount; i++)
            {
                Collider other = overlapResults[i];

                if (!other)
                    continue;

                Growable thisTree = other.GetComponent<Growable>();
                if (!thisTree) continue;
                
                if (thisTree.ripeFruitCount > 0) {
                    candidates.Add(thisTree);
                }
            }

            if (candidates.Count > 0) target = candidates[Random.Range(0, candidates.Count)];
            else target = null;
        }
    }
}
