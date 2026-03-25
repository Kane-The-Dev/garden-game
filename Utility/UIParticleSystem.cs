using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIParticleSystem : MonoBehaviour
{
    public GameObject particlePrefab;
    public Transform canvas;
    public float minSpeed, maxSpeed, gravity, rotation, minFlip, maxFlip, minLifetime, maxLifetime;
    public int minCount, maxCount;
    public RectTransform target;

    void ApplySettings(UIParticle p)
    {
        Vector2 dir = Random.insideUnitCircle.normalized;
        float speed = Random.Range(minSpeed, maxSpeed);
        p.velocity = dir * speed;

        p.gravity = gravity;
        p.spinSpeed = Random.Range(-rotation, rotation);
        p.flipSpeed = Random.Range(minFlip, maxFlip);
        p.lifetime = Random.Range(minLifetime, maxLifetime);

        if (target) p.target = target;
    }

    public void Burst()
    {
        int count = Random.Range(minCount, maxCount);
        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(particlePrefab, canvas);
            UIParticle p = obj.GetComponent<UIParticle>();

            ApplySettings(p);
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
            ApplySettings(p);
        }
    }

    public void Emission(float delay)
    {
        StartCoroutine(EmitOverTime(delay));
    }

    IEnumerator EmitOverTime(float delay)
    {
        int count = Random.Range(minCount, maxCount);
        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(particlePrefab, canvas);
            UIParticle p = obj.GetComponent<UIParticle>();

            ApplySettings(p);

            yield return new WaitForSeconds(delay);
        }
    }
}
