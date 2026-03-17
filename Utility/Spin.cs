using UnityEngine;

public class Spin : MonoBehaviour
{
    [Header("Rotate Axis")]
    public bool x;
    public bool y = true;
    public bool z;

    [Header("Settings")]
    public float speed = 90f; // rotating speed (deg/sec)
    public float acceleration = 5f;

    private float currentSpeed = 0f;

    void Update()
    {
        Vector3 axis = new Vector3(
            x ? 1f : 0f,
            y ? 1f : 0f,
            z ? 1f : 0f
        );

        if (axis == Vector3.zero) return;

        currentSpeed = Mathf.Lerp(currentSpeed, speed, acceleration * Time.deltaTime);

        transform.Rotate(axis.normalized * currentSpeed * Time.deltaTime, Space.Self);
    }
}