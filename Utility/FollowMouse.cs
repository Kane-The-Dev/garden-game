using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    public UIParticleSystem myEffect;

    void Update()
    {
        transform.position = Input.mousePosition;
    }
}
