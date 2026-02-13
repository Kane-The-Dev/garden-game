using UnityEngine;

public class Spin : MonoBehaviour
{
    [Header("Rotate Axis")]
    public bool x;
    public bool y = true;
    public bool z;

    [Header("Settings")]
    public float speed = 90f; // degrees per second

    void Update()
    {
        Vector3 axis = new Vector3(
            x ? 1f : 0f,
            y ? 1f : 0f,
            z ? 1f : 0f
        );

        transform.Rotate(axis * speed * Time.deltaTime, Space.Self);
    }
}

