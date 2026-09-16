using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSource : MonoBehaviour
{
    [SerializeField] Light myLight;
    [SerializeField] ParticleSystem main, sub;

    void Start()
    {
        if (!myLight) myLight = GetComponent<Light>();

        DayNightController dnc = GameManager.instance.clock;
        dnc.RegisterLight(myLight);

        if (main) dnc.RegisterParticle(main);
        if (sub) dnc.RegisterParticle(sub);
    }
}
