using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildTool : MonoBehaviour
{
    public int buildID;
    float maxDistance = 100f, rotY;
    public bool available; // current point is available to build on
    [SerializeField] string[] searchFolders = { "Buildings" };
    [SerializeField] Material previewMaterial;
    Color currentColor;
    [SerializeField] Color valid, notValid;
    Collider previewCollider;
    Collider[] overlapResults = new Collider[16];
    Inventory inventory;

    void Start() 
    {
        currentColor = Color.white;
        inventory = GameManager.instance.inventory;
        buildID = -1;
    }

    public GameObject SpawnPreview()
    {
        GameObject prefab = LoadBuildingPrefab(inventory.buildingList[buildID].name);
        if (prefab == null)
        {
            Debug.LogWarning($"No building prefab found for '{inventory.buildingList[buildID].name}' in Resources/{searchFolders}");
            return null;
        }

        GameObject newPreview = Instantiate(prefab);
        
        foreach (Renderer r in newPreview.GetComponentsInChildren<Renderer>())
            r.sharedMaterial = previewMaterial;

        foreach (Collider c in newPreview.GetComponentsInChildren<Collider>())
        {
            c.enabled = true;
            c.isTrigger = true;
        }

        previewCollider = newPreview.GetComponentInChildren<Constructible>().GetComponent<Collider>();
        return newPreview;
    }

    public void RotatePreview(GameObject preview, int clockwise)
    {
        rotY += clockwise * 90f * Time.deltaTime;
        preview.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
    }

    public void BuildCheck(GameObject preview, Ray ray, LayerMask gMask, LayerMask oMask)
    {
        if (buildID < 0) return;

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, gMask))
        {
            if (preview) preview.transform.position = new Vector3(hit.point.x, hit.point.y, hit.point.z);

            bool blocked = false;

            // count the number of colliders within range
            float checkRadius = previewCollider.bounds.extents.magnitude;

            int hitCount = Physics.OverlapSphereNonAlloc(
                previewCollider.bounds.center,
                checkRadius,
                overlapResults,
                oMask,
                QueryTriggerInteraction.Collide
            );

            // check each individual colliders if they are actually colliding
            for (int i = 0; i < hitCount; i++)
            {
                Collider other = overlapResults[i];

                if (!other)
                    continue;

                if (other.transform.IsChildOf(preview.transform))
                    continue;

                if (Physics.ComputePenetration(
                    previewCollider,
                    preview.transform.position,
                    preview.transform.rotation,
                    other,
                    other.transform.position,
                    other.transform.rotation,
                    out Vector3 _, out float _
                )) {
                    // Debug.Log("blocked!");
                    blocked = true;
                    break;
                }
            }

            available = !blocked;
            Color targetColor = blocked ? notValid : valid;

            // Smooth color transition
            currentColor = Color.Lerp(
                currentColor,
                targetColor,
                Time.deltaTime * 20f
            );
            
            previewMaterial.color = currentColor;
        }
    }

    public void BuildConfirm(Ray ray, LayerMask gMask, LayerMask oMask)
    {
        if (buildID < 0) return;

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, gMask))
        {
            if (!available) return;

            string buildName = inventory.buildingList[buildID].name;
            if (inventory.myInventory[buildName] <= 0)
            {
                Debug.Log("Out of seed/item!");
                return;
            }

            if (buildID < 0) return;
            else Build(hit.point);

            inventory.myInventory[buildName]--;
            inventory.exp += 25f;

            inventory.selection.RefreshBuildings();
        }
    }

    void Build(Vector3 point)
    {
        GameObject prefab = LoadBuildingPrefab(inventory.buildingList[buildID].name);
        if (prefab == null)
        {
            Debug.LogWarning($"No building prefab found for '{inventory.buildingList[buildID].name}' in Resources/{searchFolders}");
            return;
        }

        GameObject newBuilding = Instantiate(
            prefab,
            point, 
            Quaternion.Euler(0f, rotY, 0f)
        );

        Constructible constructible = newBuilding.GetComponentInChildren<Constructible>();
        if (constructible)
            constructible.isPreview = false;
    }

    GameObject LoadBuildingPrefab(string itemName)
    {
        if (string.IsNullOrEmpty(itemName))
            return null;

        string nameWithoutSpaces = itemName.Replace(" ", string.Empty);

        // Try the direct name first, mirroring the plant tool pattern.
        foreach (string path in new[] { itemName, nameWithoutSpaces })
        {
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab != null)
                return prefab;
        }

        // Then try each configured folder in order.
        if (searchFolders != null)
        {
            foreach (string folder in searchFolders)
            {
                if (string.IsNullOrWhiteSpace(folder))
                    continue;

                string normalizedFolder = folder.Trim().Trim('/');
                if (normalizedFolder.StartsWith("Resources/", System.StringComparison.OrdinalIgnoreCase))
                    normalizedFolder = normalizedFolder.Substring("Resources/".Length);

                foreach (string path in new[]
                         {
                             normalizedFolder + "/" + itemName,
                             normalizedFolder + "/" + nameWithoutSpaces
                         })
                {
                    GameObject prefab = Resources.Load<GameObject>(path);
                    if (prefab != null)
                        return prefab;
                }
            }
        }

        return null;
    }
}
