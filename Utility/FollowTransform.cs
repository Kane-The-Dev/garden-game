using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    RectTransform myRect;
    public bool isRect = false;

    [Header("Target")]
    public Transform target;
    public RectTransform rectTarget;

    [Header("Follow")]
    public bool followPosition = true;
    public bool followRotation = false;
    public bool followScale = false;

    [Header("Speed")]
    public float positionSpeed = 10f;
    public float rotationSpeed = 10f;
    public float scaleSpeed = 10f;

    [Header("Offset")]
    public Vector3 positionOffset = new Vector3(10,0,0);
    public Vector3 rotationOffset;
    public Vector3 scaleOffset;

    void Start()
    {
        if (isRect) myRect = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (!isRect && !target) return;
        if (isRect && !rectTarget) return;

        float deltaTime = Time.deltaTime;

        if (followPosition)
        {
            if (isRect)
            {
                Vector3 targetPosition = rectTarget.position + positionOffset;

                myRect.position = positionSpeed <= 0f
                    ? targetPosition
                    : Vector3.Lerp(myRect.position, targetPosition, positionSpeed * deltaTime);
            }
            else
            {
                Vector3 targetPosition = target.position + positionOffset;

                transform.position = positionSpeed <= 0f
                    ? targetPosition
                    : Vector3.Lerp(transform.position, targetPosition, positionSpeed * deltaTime);
            }
        }

        if (followRotation)
        {
            Quaternion targetRotation = (isRect ? rectTarget.rotation : target.rotation) * Quaternion.Euler(rotationOffset);

            transform.rotation = rotationSpeed <= 0f
                ? targetRotation
                : Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * deltaTime);
        }

        if (followScale)
        {
            Vector3 targetScale = (isRect ? rectTarget.localScale : target.localScale) + scaleOffset;

            transform.localScale = scaleSpeed <= 0f
                ? targetScale
                : Vector3.Lerp(transform.localScale, targetScale, scaleSpeed * deltaTime);
        }
    }
}
