using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    [Header("Info")]
    public int ID;

    [Header("Other")]
    public Transform drop;
    [SerializeField] Spin[] wheels;
    public Rigidbody rb;
    [SerializeField] AudioClip landing, starting; // unique engine starting sound
    AudioSource source;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        source = GetComponent<AudioSource>();
    }

    public void CollideGround()
    {
        source.PlayOneShot(landing);
    }

    public void StartEngine()
    {
        if (source && starting) 
            source.PlayOneShot(starting);

        foreach (Spin wheel in wheels)
            wheel.speed = 180f;
    }

    public void Move(Vector3 direction) 
    {
        rb.constraints = RigidbodyConstraints.None;
        rb.AddForce(direction, ForceMode.Impulse);
    }
}
