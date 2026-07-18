using System.Collections;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 8f;
    [SerializeField] float acceleration = 12f;
    [SerializeField] float deceleration = 16f;
    public float movementX, movementZ, factor;
    public bool movable, targetReached;
    public Transform root, target;
    Rigidbody rb;
    [SerializeField] float restrictedRadius;
    private Vector3 currentVelocity;

    [Header("Shake Settings")]
    [SerializeField] float duration = 0.4f;
    [SerializeField] float frequency = 25f;
    [SerializeField] float damping = 5f;

    void Start()
    {
        rb = transform.parent.GetComponent<Rigidbody>();
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
            root.position = Vector3.Lerp(
                root.position, 
                target.position, 
                Time.deltaTime * 5f
            );
            root.rotation = Quaternion.Slerp(
                root.rotation, 
                target.rotation, 
                Time.deltaTime * 5f
            );

            // Stop if close enough
            if (Vector3.Distance(root.position, target.position) < 0.1f &&
                Quaternion.Angle(root.rotation, target.rotation) < 0.5f) {
                targetReached = true;
            }
        }

        if (!movable || !targetReached) return;

        movementX = Input.GetAxisRaw("Horizontal");
        movementZ = Input.GetAxisRaw("Vertical");
        Vector3 inputDirection = new Vector3(movementX, 0f, movementZ).normalized;

        Vector3 projectedForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 projectedRight = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;

        Vector3 moveDirection = (projectedForward * inputDirection.z + projectedRight * inputDirection.x);
        Vector3 targetVelocity = moveDirection * speed * factor;

        if (inputDirection.sqrMagnitude > 0.001f)
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, Time.deltaTime * acceleration);
        else
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, Time.deltaTime * deceleration);

        rb.velocity = currentVelocity;
    }

    public void ScreenShake(float amplitude)
    {
        StartCoroutine(Shake(0.2f));
    }
    IEnumerator Shake(float amplitude)
    {
        float time = 0f;

        Vector3 dir = Random.onUnitSphere;

        while (time < duration)
        {
            time += Time.deltaTime;

            float decay = Mathf.Exp(-damping * time);
            float wave = Mathf.Sin(time * frequency);

            Vector3 offset = dir * amplitude * wave * decay;
            transform.localPosition = offset;

            yield return null;
        }

        transform.localPosition = Vector3.zero;
    }
}
