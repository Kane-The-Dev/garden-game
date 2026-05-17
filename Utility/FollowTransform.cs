using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    public Transform target;

    [Header("Follow")]
    [SerializeField] bool followPosition = true;
    [SerializeField] bool followRotation = true;
    [SerializeField] bool followScale = false;

    [Header("Speed")]
    [SerializeField] float positionSpeed = 10f;
    [SerializeField] float rotationSpeed = 10f;
    [SerializeField] float scaleSpeed = 10f;

    void Update()
    {
        if (!target) return;

        float deltaTime = Time.deltaTime;

        if (followPosition)
            transform.position = Vector3.Lerp(
                transform.position,
                target.position,
                positionSpeed * deltaTime
            );

        if (followRotation)
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                target.rotation,
                rotationSpeed * deltaTime
            );

        if (followScale)
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                target.localScale,
                scaleSpeed * deltaTime
            );
    }
}
