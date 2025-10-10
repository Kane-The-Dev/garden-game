using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float speed, movementX, movementZ;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        movementX = Input.GetAxisRaw("Horizontal");
        movementZ = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(movementX, 0f, movementZ).normalized;
        rb.velocity = direction * speed;
    }
}
