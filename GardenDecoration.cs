using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GardenDecoration : MonoBehaviour
{
    [SerializeField] private GameObject[] decorations, trees;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] Vector3 center;
    [SerializeField] float radius;
    [SerializeField] int minCount, maxCount;
    float maxDistance = 100f;
    int n1, n2;
    
    void Start()
    {
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
                groundMask, 
                QueryTriggerInteraction.Collide
            )) 
            {
                if (hit.collider.CompareTag("Obstacle"))
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
            }
        }
    }
}
