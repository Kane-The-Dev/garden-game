using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float speed, movementX, movementZ;
    public bool movable, targetReached;
    public Transform target;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        movable = true;
    }

    void Update()
    {
        if (!targetReached && target != null)
        {
            transform.position = Vector3.Lerp(
                transform.position, 
                target.position, 
                Time.deltaTime * 5f
            );
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                target.rotation, 
                Time.deltaTime * 5f
            );

            // Stop if close enough
            if (Vector3.Distance(transform.position, target.position) < 0.01f &&
                Quaternion.Angle(transform.rotation, target.rotation) < 0.5f) {
                targetReached = true;
            }
        }

        if (!movable) return;

        movementX = Input.GetAxisRaw("Horizontal");
        movementZ = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(movementX, 0f, movementZ).normalized;
        rb.velocity = direction * speed;
    }
}
