using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Constructible))]
public class AutoHarvester : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] float scanRadius = 15f;
    [SerializeField] float speed = 5f, scanCooldown = 5f, harvestCooldown = 2f, harvestRange;

    [Header("References")]
    [SerializeField] LayerMask plantMask;
    [SerializeField] Transform helper;
    [SerializeField] Growable target;
    [SerializeField] Transform home;
    public bool CanStart => constructible && !constructible.isPreview;

    float timer = 0f;
    Collider[] overlapResults = new Collider[16];
    Vector3? patrolTarget = null;
    Animator animator;
    Constructible constructible;
    GameManager gm;
    
    void Awake()
    {
        animator = GetComponent<Animator>();
        constructible = GetComponent<Constructible>();
    }

    void Start()
    {
        gm = GameManager.instance;

        if (CanStart)
        {
            helper.GetComponent<Rigidbody>().useGravity = true;
            helper.GetComponent<Collider>().enabled = true;
        }

        StartCoroutine(ScanPlants());
    }

    void Update()
    {
        if (!CanStart || !helper) return;

        if (gm.clock.time > 0.25f && gm.clock.time < 0.75f) // go home at night
        {
            if (Vector3.Distance(helper.position, home.position) > 1f) 
                MoveTo(home.position);
            else
                animator.SetBool("isMoving", false);
        }
        else if (target) // follow target if available
        {
            if (Vector3.Distance(helper.position, target.transform.position) > harvestRange)
            {
                MoveTo(target.transform.position);
            }
            else if (timer <= 0f)
            {
                animator.SetBool("isMoving", false);
                animator.SetTrigger("harvest");
                timer = harvestCooldown * Random.Range(0.9f, 1.1f);
                if (target.ripeFruitCount <= 0) target = null;
            }
        }
        else if (patrolTarget.HasValue) // move to a random place within scan radius
        {
            MoveTo(patrolTarget.Value);
        }
        else animator.SetBool("isMoving", false);

        if (timer > 0f) timer -= Time.deltaTime;
    }

    void MoveTo(Vector3 destination)
    {
        Vector3 direction = (destination - helper.position).normalized;

        // Move
        helper.position = Vector3.MoveTowards(
            helper.position,
            destination,
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

    public void Harvest()
    {
        if (!CanStart) return;
        if (!target || Vector3.Distance(helper.position, target.transform.position) > harvestRange)
        {
            if (target) Debug.Log(Vector3.Distance(helper.position, target.transform.position));
            return;  
        }
        
        target.HarvestFruit(1);
        target.Shake(4f);
    }

    IEnumerator ScanPlants()
    {
        while (true)
        {
            if (!CanStart) yield break;

            if (gm.clock.time > 0.25f && gm.clock.time < 0.75f) 
            {
                yield return new WaitForSeconds(scanCooldown * Random.Range(0.9f, 1.1f));
                continue;
            }

            // 1. Play scan animation and wait
            animator.SetTrigger("scan");
            yield return new WaitForSeconds(scanCooldown * Random.Range(0.9f, 1.1f));

            // 2. Search for ripe targets within range
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
                if (!other) continue;

                Growable thisTree = other.GetComponent<Growable>();
                if (thisTree && thisTree.ripeFruitCount > 0)
                    candidates.Add(thisTree);
            }

            if (candidates.Count > 0)
            {
                // 3a. Target found -> wait until it has been fully harvested or lost
                target = candidates[Random.Range(0, candidates.Count)];
                harvestRange = target.harvestRange;

                yield return new WaitUntil(() => target == null || (gm.clock.time > 0.25f && gm.clock.time < 0.75f));
                target = null; // clear target lock if we exited due to nightfall
            }
            else
            {
                // 3b. No target -> patrol to a random point inside the scan circle, then scan again
                Vector3 randomOffset = Random.insideUnitSphere * scanRadius;
                randomOffset.y = 0f;
                patrolTarget = transform.position + randomOffset;

                float walkTimer = 0f;
                while (patrolTarget.HasValue
                    && Vector3.Distance(helper.position, patrolTarget.Value) > 0.2f
                    && walkTimer < 5f)
                {
                    walkTimer += Time.deltaTime;
                    yield return null;
                }

                patrolTarget = null;
                animator.SetBool("isMoving", false);
            }
        }
    }
}
