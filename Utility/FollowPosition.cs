using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPosition : MonoBehaviour
{
    public RectTransform target;
    RectTransform rect;
    [SerializeField] Vector3 offset;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        rect.position = target.position + offset;
    }
}
