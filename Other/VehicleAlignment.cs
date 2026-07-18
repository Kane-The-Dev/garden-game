using UnityEngine;

public class VehicleAlignment : MonoBehaviour
{
    public Transform pivot;
    public Transform frontLeft;
    public Transform frontRight;
    public Transform rearLeft;
    public Transform rearRight;

    public float rayLength = 3f;
    public LayerMask groundMask;
    public float rotationSmooth = 1f;

    Vector3[] hitPoints = new Vector3[4];

    void Update()
    {
        if (CastAllWheels())
        {
            Vector3 averagePoint = GetAverage(hitPoints);

            // transform.position = averagePoint;

            UpdateRotation();
        }
    }

    bool CastAllWheels()
    {
        Transform[] wheels = { frontLeft, frontRight, rearLeft, rearRight };

        for (int i = 0; i < wheels.Length; i++)
        {
            if (Physics.Raycast(wheels[i].position, Vector3.down, out RaycastHit hit, rayLength, groundMask))
            {
                hitPoints[i] = hit.point;
                Debug.DrawLine(wheels[i].position, hit.point, Color.green);
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    Vector3 GetAverage(Vector3[] points)
    {
        Vector3 sum = Vector3.zero;

        for (int i = 0; i < points.Length; i++)
            sum += points[i];

        return sum / points.Length;
    }

    void UpdateRotation()
    {
        Vector3 diag1 = hitPoints[1] - hitPoints[2]; // FR - RL
        Vector3 diag2 = hitPoints[0] - hitPoints[3]; // FL - RR

        Vector3 normal = Vector3.Cross(diag1, diag2).normalized;

        Vector3 forwardProjected = Vector3.ProjectOnPlane(pivot.forward, normal).normalized;

        Quaternion targetRotation = Quaternion.LookRotation(forwardProjected, normal);

        pivot.rotation = Quaternion.Slerp(
            pivot.rotation,
            targetRotation,
            Time.deltaTime * rotationSmooth
        );
    }
}