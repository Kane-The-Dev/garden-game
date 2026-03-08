using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIParticleSystem : MonoBehaviour
{
    [SerializeField] GameObject particlePrefab;
    [SerializeField] Transform canvas;
    [SerializeField] float minSpeed, maxSpeed, gravity, rotation, minLifetime, maxLifetime;
    [SerializeField] int minCount, maxCount;

    public void Burst()
    {
        int count = Random.Range(minCount, maxCount);
        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(particlePrefab, canvas);
            UIParticle p = obj.GetComponent<UIParticle>();

            Vector2 dir = Random.insideUnitCircle.normalized;
            float speed = Random.Range(minSpeed, maxSpeed);
            p.velocity = dir * speed;

            p.gravity = gravity;
            p.spinSpeed = Random.Range(-rotation, rotation);
            p.lifetime = Random.Range(minLifetime, maxLifetime);
        }
    }
}
