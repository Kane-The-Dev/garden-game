using System.Collections;
using UnityEngine;

public class FoodDrop : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] foodPrefabs;
    [Range(0.1f, 1f)]
    public float spawnInterval = 0.5f;
    public float spawnHeightOffset = 0.5f;

    private void Start()
    {
        StartCoroutine(SpawnFoodRoutine());
    }

    private IEnumerator SpawnFoodRoutine()
    {
        while (true)
        {
            SpawnRandomFood();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnRandomFood()
    {
        if (foodPrefabs == null || foodPrefabs.Length == 0)
            return;

        GameObject prefabToSpawn = foodPrefabs[Random.Range(0, foodPrefabs.Length)];
        Vector3 spawnPosition = transform.position + Vector3.up * spawnHeightOffset;

        GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        spawnedObject.transform.localScale *= 2f;

        Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;
            rb.drag = 8f;
            rb.angularDrag = 0.5f;
        }

        Growable growable = spawnedObject.GetComponent<Growable>();
        if (growable != null)
        {
            growable.enabled = false;
        }

        Destroy(spawnedObject, 5f);
    }
}
