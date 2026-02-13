using UnityEngine;

public class FaceToCamera : MonoBehaviour
{
    [Header("Lock Axis")]
    public bool lockX = true;
    public bool lockY = false;
    public bool lockZ = true;

    CameraMovement mainCam;

    void Start()
    {
        mainCam = GameManager.instance.cam;
    }

    void Update()
    {
        Vector3 direction = transform.position - mainCam.transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        Vector3 euler = lookRotation.eulerAngles;
        Vector3 currentEuler = transform.rotation.eulerAngles;

        if (lockX) euler.x = currentEuler.x;
        if (lockY) euler.y = currentEuler.y;
        if (lockZ) euler.z = currentEuler.z;

        transform.rotation = Quaternion.Euler(euler);
    }
}
