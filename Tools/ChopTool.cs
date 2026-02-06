using System.Collections;
using UnityEngine;

public class ChopTool : MonoBehaviour
{
    float maxDistance = 100f;
    [SerializeField] float speed;

    Inventory inventory;

    void Start() 
    {
        inventory = FindObjectOfType<Inventory>();
    }

    public void ChopTree(Ray ray, LayerMask mask)
    {
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, mask))
        {
            Growable thisTree = hit.collider.gameObject.GetComponent<Growable>();
            if (!thisTree) return;

            thisTree.chopIndex += speed * Time.deltaTime;
        }
        // else
        // {
        //     Debug.Log("No hit detected");
        // }
    }
}
