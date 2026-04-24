using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildTool : MonoBehaviour
{
    public int buildID;
    float maxDistance = 100f, radius = 0.5f, rotY;
    [SerializeField] GameObject[] buildings, previews;
    Color currentColor;
    Renderer ringRender;
     
    Inventory inventory;

    void Start() 
    {
        currentColor = Color.white;
        inventory = GameManager.instance.inventory;
        buildID = -1;
    }

    public GameObject SpawnPreview()
    {
        GameObject newPreview = Instantiate(previews[buildID]);
        foreach (Collider c in newPreview.GetComponentsInChildren<Collider>())
            c.enabled = false;
        return newPreview;
    }

    public void RotatePreview(GameObject preview)
    {
        rotY += 45f * Time.deltaTime;
        preview.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
    }

    public void BuildCheck(GameObject preview, GameObject ring, Ray ray, LayerMask gMask, LayerMask oMask)
    {
        if (buildID < 0) return;

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, gMask))
        {
            ring.transform.localScale = new Vector3(0.2f * radius, 1f, 0.2f * radius);
            ring.transform.position = new Vector3(hit.point.x, hit.point.y + 0.1f, hit.point.z);
            if (preview) preview.transform.position = new Vector3(hit.point.x, hit.point.y + 0.1f, hit.point.z);

            bool blocked = Physics.CheckSphere(
                hit.point,
                radius,
                oMask,
                QueryTriggerInteraction.Collide
            );

            Color targetColor = blocked
                ? new Color(1f, 0f, 0f, 0.8f)
                : new Color(1f, 1f, 1f, 0.8f);

            // Smooth transition
            currentColor = Color.Lerp(
                currentColor,
                targetColor,
                Time.deltaTime * 20f
            );
            
            if (!ringRender) ringRender = ring.GetComponent<Renderer>();
            ringRender.material.color = currentColor;
        }
    }

    public void BuildConfirm(Ray ray, LayerMask gMask, LayerMask oMask)
    {
        if (buildID < 0) return;

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, gMask))
        {
            if (Physics.CheckSphere(hit.point, radius, oMask, QueryTriggerInteraction.Collide)) return;

            string plantName = inventory.buildingList[buildID].name;
            if (inventory.myInventory[plantName] <= 0)
            {
                Debug.Log("Out of seed/item!");
                return;
            }

            if (buildID < 0) return;
            else Build(hit.point);

            inventory.myInventory[plantName]--;
            inventory.exp += 25f;

            inventory.selection.RefreshBuildings();
        }
    }

    void Build(Vector3 point)
    {
        GameObject newBuilding = Instantiate(
            buildings[buildID], 
            point, 
            Quaternion.Euler(0f, Random.Range(0f, 180f), 0f)
        );
    }
}
