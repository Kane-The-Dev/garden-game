using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GardenDecoration : MonoBehaviour
{
    [Header("Decoration")]
    [SerializeField] GameObject[] decorations, trees;
    [SerializeField] LayerMask groundMask, obstacleMask;
    [SerializeField] Vector3 center;
    [SerializeField] float radius;
    [SerializeField] int minCount, maxCount;

    [Header("Fences")]
    [SerializeField] Animator fenceAnimator;

    float maxDistance = 100f;
    int n1, n2;
    List<GameObject> spawned = new List<GameObject>();

    GameManager gm;
    
    void Start()
    {
        gm = GameManager.instance;
        n1 = decorations.Length;
        n2 = trees.Length;
        InitializeGarden();
    }

    void InitializeGarden()
    {
        int n = Random.Range(minCount, maxCount);

        for (int i = 0; i < n; i++)
        {
            Vector3 point = center + Random.insideUnitSphere * radius;

            if (Physics.Raycast(
                point, 
                Vector3.down, 
                out RaycastHit hit, 
                maxDistance, 
                groundMask
            )) 
            {
                if (Physics.CheckSphere(hit.point, 0.1f, obstacleMask, QueryTriggerInteraction.Collide))
                {
                    i--;
                    continue;
                }

                float dist = Vector3.Distance(hit.point, center);

                GameObject decor;
                int ID;

                if (dist >= radius * 0.5f)
                {
                    // trees allowed
                    ID = Random.Range(0, n1 + n2);

                    if (ID < n1)
                        decor = Instantiate(
                            decorations[ID],
                            hit.point,
                            Quaternion.Euler(0f, Random.Range(-90f, 90f), 0f)
                        );
                    else
                        decor = Instantiate(
                            trees[ID - n1],
                            hit.point,
                            Quaternion.Euler(0f, Random.Range(-90f, 90f), 0f)
                        );
                }
                else
                {
                    // center area → decorations only
                    ID = Random.Range(0, n1);

                    decor = Instantiate(
                        decorations[ID],
                        hit.point,
                        Quaternion.Euler(0f, Random.Range(-90f, 90f), 0f)
                    );
                }

                decor.transform.localScale *= Random.Range(0.8f, 1.2f);
                decor.transform.parent = this.transform;
                spawned.Add(decor);
            }
        }

        foreach (GameObject obj in spawned)
        {
            obj.GetComponent<SphereCollider>().enabled = false;
        }
    }

    public void UpgradeGarden()
    {
        fenceAnimator.SetTrigger("Upgrade!");
        gm.sm.shopPanel.gameObject.SetActive(false);
        gm.ChangeMode(2);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Invoke("FinishUpgrade", 8f);
    }

    void FinishUpgrade()
    {
        gm.ChangeMode(0);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
