using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildTool : MonoBehaviour
{
    public int buildID;
    float maxDistance = 100f, rotY;
    public bool available; // current point is available to build on
    [SerializeField] GameObject[] buildings;
    [SerializeField] Material previewMaterial;
    Renderer previewRender;
    Color currentColor;
    [SerializeField] Color valid, notValid;
    [SerializeField] Collider previewCollider;
    [SerializeField] Collider[] overlapResults = new Collider[16];
    Inventory inventory;

    void Start() 
    {
        currentColor = Color.white;
        inventory = GameManager.instance.inventory;
        buildID = -1;
    }

    public GameObject SpawnPreview()
    {
        GameObject newPreview = Instantiate(buildings[buildID]);
        foreach (Renderer r in newPreview.GetComponentsInChildren<Renderer>())
            r.sharedMaterial = previewMaterial;
        foreach (Collider c in newPreview.GetComponentsInChildren<Collider>())
        {
            c.enabled = true;
            c.isTrigger = true;
        }

        previewCollider = newPreview.transform.GetChild(0).GetComponent<Collider>();
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
                    Debug.Log("blocked!");
                    blocked = true;
                    break;
                }
            }

            available = !blocked;
            Color targetColor = blocked ? notValid : valid;

            // Smooth transition
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
        GameObject newBuilding = Instantiate(
            buildings[buildID],
            point, 
            Quaternion.Euler(0f, rotY, 0f)
        );

        newBuilding.transform.GetChild(0).GetComponent<Constructible>().isPreview = false;
    }
}
