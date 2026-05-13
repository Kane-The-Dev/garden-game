using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoHarvester : MonoBehaviour
{
    [SerializeField] float scanRadius = 15f, speed = 5f, scanCooldown = 5f, harvestCooldown = 2f, harvestRange;
    [SerializeField] LayerMask plantMask;
    [SerializeField] Transform helper;
    float timer = 0f;
    Collider[] overlapResults = new Collider[16];
    [SerializeField] Growable target;
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(ScanPlants());
    }

    void Update()
    {
        if (target)
        {
            if (Vector3.Distance(helper.position, target.transform.position) > harvestRange)
            {
                Vector3 direction = (target.transform.position - helper.position).normalized;

                // Move
                helper.position = Vector3.MoveTowards(
                    helper.position,
                    target.transform.position,
                    speed * Time.deltaTime
                );

                // Rotate
                direction.y = 0f;
                direction.Normalize();

                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    targetRotation *= Quaternion.Euler(0, -90, 0);

                    helper.rotation = Quaternion.Slerp(
                        helper.rotation,
                        targetRotation,
                        15f * Time.deltaTime
                    );
                }
                animator.SetBool("isMoving", true);
            }
            else if (timer <= 0f)
            {
                animator.SetBool("isMoving", false);
                animator.SetTrigger("harvest");
                timer = harvestCooldown;
                if (target.ripeFruitCount <= 0) target = null;
            }
        }
        else {
            animator.SetBool("isMoving", false);
        }

        if (timer > 0f) timer -= Time.deltaTime;
    }

    public void Harvest()
    {
        if (!target || Vector3.Distance(helper.position, target.transform.position) > harvestRange) return;
        target.HarvestFruit(2);
        target.Shake(4f);
    }

    IEnumerator ScanPlants()
    {
        while (true)
        {
            if (!target) animator.SetTrigger("scan");
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

            if (candidates.Count > 0) {
                target = candidates[Random.Range(0, candidates.Count)];
                harvestRange = target.harvestRange;
            }
            else target = null;
        }
    }
}
