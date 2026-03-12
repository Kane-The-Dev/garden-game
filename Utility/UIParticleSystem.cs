using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIParticleSystem : MonoBehaviour
{
    [SerializeField] GameObject particlePrefab;
    [SerializeField] Transform canvas;
    [SerializeField] float minSpeed, maxSpeed, gravity, rotation, minFlip, maxFlip, minLifetime, maxLifetime;
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
            p.flipSpeed = Random.Range(minFlip, maxFlip);
            p.lifetime = Random.Range(minLifetime, maxLifetime);
        }
    }

    public void Burst(string msg)
    {
        int count = Random.Range(minCount, maxCount);
        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(particlePrefab, canvas);
            UIParticle p = obj.GetComponent<UIParticle>();

            p.text.text = msg;

            Vector2 dir = Random.insideUnitCircle.normalized;
            float speed = Random.Range(minSpeed, maxSpeed);
            p.velocity = dir * speed;

            p.gravity = gravity;
            p.spinSpeed = Random.Range(-rotation, rotation);
            p.flipSpeed = Random.Range(minFlip, maxFlip);
            p.lifetime = Random.Range(minLifetime, maxLifetime);
        }
    }
}
