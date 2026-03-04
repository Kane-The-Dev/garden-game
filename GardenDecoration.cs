using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GardenDecoration : MonoBehaviour
{
    [SerializeField] private GameObject[] decorations;
    [SerializeField] private LayerMask groundMask;
    float maxDistance = 100f;
    
    void Start()
    {
        InitializeGarden();
    }

    void InitializeGarden()
    {
        int n = Random.Range(20, 30);
        float randomX, randomZ;

        for (int i = 0; i < n; i++)
        {
            randomX = Random.Range(-3f, 23f);
            randomZ = Random.Range(-3f, 23f);

            Vector3 point = new Vector3(randomX, 10f, randomZ);

            if (Physics.Raycast(point, Vector3.down, out RaycastHit hit, maxDistance, groundMask)) {
                
                if (hit.collider.CompareTag("Obstacle")) {
                    i--;
                    continue;
                }

                GameObject decor = Instantiate(
                    decorations[Random.Range(0, decorations.Length)], 
                    hit.point, 
                    Quaternion.Euler(0f, Random.Range(-90f, 90f), 0f)
                );

                decor.transform.localScale *= Random.Range(0.8f, 1f);
            }
        }
    }
}
