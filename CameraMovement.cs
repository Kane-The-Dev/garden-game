using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float speed, movementX, movementZ, factor;
    public bool movable, targetReached;
    public Transform target;
    Rigidbody rb;
    [SerializeField] float restrictedRadius;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        targetReached = true;
        movable = true;
    }

    void Update()
    {
        Vector2 myXZ = new Vector2(transform.position.x, transform.position.z);
        float dist = Vector2.Distance(myXZ, Vector2.zero);
        float slowRadius = restrictedRadius - 5f;
        factor = Mathf.InverseLerp(restrictedRadius, slowRadius, dist);
        factor = Mathf.Max(factor, 0.05f);

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
            if (Vector3.Distance(transform.position, target.position) < 0.1f &&
                Quaternion.Angle(transform.rotation, target.rotation) < 0.5f) {
                targetReached = true;
            }
        }

        if (!movable || !targetReached) return;

        movementX = Input.GetAxisRaw("Horizontal");
        movementZ = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(movementX, 0f, movementZ).normalized;
        rb.velocity = direction * speed * factor;
    }
}
