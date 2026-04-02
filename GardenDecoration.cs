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

    // wind settings
    Dictionary<GameObject, float> offset = new Dictionary<GameObject, float>();
    Dictionary<GameObject, float> amplitude = new Dictionary<GameObject, float>();

    GameManager gm;
    
    void Start()
    {
        gm = GameManager.instance;
        n1 = decorations.Length;
        n2 = trees.Length;
        InitializeGarden();
    }

    void Update()
    {
        UpdateGarden();
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

                float random = Random.Range(4.5f, 5.4f);
                if (decor.name.Contains("Tree")) random *= 0.2f;
                else if (decor.name.Contains("Rock")) random *= 0f;

                amplitude[decor] = random;
                offset[decor] = Random.Range(0f, 90f);
            }
        }

        foreach (GameObject obj in spawned)
        {
            obj.GetComponent<SphereCollider>().enabled = false;
        }
    }

    void UpdateGarden()
    {
        foreach (GameObject obj in spawned)
        {
            if (obj == null) continue;

            float rotX = Mathf.Sin(Time.time * 0.5f + offset[obj]) * amplitude[obj];
            float rotZ = Mathf.Sin(Time.time * 0.5f + offset[obj]) * amplitude[obj];

            Vector3 euler = obj.transform.eulerAngles;
            obj.transform.rotation = Quaternion.Euler(rotX, euler.y, rotZ);
        }
    }
}
