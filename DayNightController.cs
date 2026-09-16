using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class LightData
{
    public Light light;
    public float maxIntensity = 1f;

    public LightData() { }

    public LightData(Light l)
    {
        light = l;
        maxIntensity = l != null ? l.intensity : 1f;
    }
}

public class DayNightController : MonoBehaviour
{
    public Light sun;

    [Header("Visualization")]
    [SerializeField] TextMeshProUGUI dayDisplay;
    [SerializeField] RectTransform clock;
    [SerializeField] float clockOffset;

    [Header("Time Settings")]
    public float time;
    public int dayCount;
    public float dayLength = 60f; // seconds for full cycle
    public float startTime = 0f;  // 0-1
    [SerializeField] float morning, evening;

    [Header("Sky Settings")]
    [SerializeField] Gradient lightColor;
    [SerializeField] Gradient skyColor;
    [SerializeField] AnimationCurve lightIntensity, skyIntensity;

    [Header("Other Light Sources")]
    [SerializeField] Material[] glowMaterials;
    [SerializeField] List<ParticleSystem> glowParticles = new List<ParticleSystem>();
    [SerializeField] public List<LightData> lights = new List<LightData>();
    [SerializeField] AnimationCurve intensityCurve;

    Material skyboxInstance;
    bool lightsOut = true;

    void Awake() 
    {
        dayCount = 0;
        time = startTime;
        skyboxInstance = new Material(RenderSettings.skybox);
        RenderSettings.skybox = skyboxInstance;
        lightsOut = time > evening && time < morning;
    }

    void Start()
    {
        InitializeLights();
    }

    public void InitializeLights()
    {
        Light[] allLights = FindObjectsOfType<Light>(true);
        lights.Clear();
        foreach (Light l in allLights)
        {
            if (l == sun || l.CompareTag("Ignore")) continue;
            lights.Add(new LightData(l));
        }
    }

    public void RegisterLight(Light l)
    {
        if (l == null || l == sun) return;
        if (lights.Exists(item => item != null && item.light == l)) return;
        lights.Add(new LightData(l));
    }

    public void UnregisterLight(Light l)
    {
        if (l == null) return;
        lights.RemoveAll(item => item == null || item.light == l);
    }

    public void RegisterParticle(ParticleSystem p) 
    {
        if (p && !glowParticles.Exists(item => item != null && item == p)) 
            glowParticles.Add(p);
    }

    void Update()
    {
        time += Time.deltaTime / dayLength;
        if (time >= 1f) {
            dayCount += 1;
            time %= 1f;
        }

        dayDisplay.text = dayCount.ToString();
        clock.rotation = Quaternion.Euler(0f, 0f, clockOffset + 360f * time);

        float value = intensityCurve.Evaluate(time);
        for (int i = lights.Count - 1; i >= 0; i--)
        {
            LightData l = lights[i];
            if (l != null && l.light != null)
                l.light.intensity = value * l.maxIntensity;
        }

        if (lightsOut && time > evening && time < morning)
        {
            foreach (Material glow in glowMaterials) 
                glow.EnableKeyword("_EMISSION");

            foreach (ParticleSystem p in glowParticles) 
                if (p) p.Play();

            lightsOut = false;
        }
        else if (!lightsOut && (time > morning || time < evening))
        {
            foreach (Material glow in glowMaterials) 
                glow.DisableKeyword("_EMISSION");

            foreach (ParticleSystem p in glowParticles) 
                if (p) p.Stop();
                
            lightsOut = true;
        }

        UpdateSun();
    }

    void UpdateSun()
    {
        if (sun == null) return;

        float sunAngle = time * 360f;

        // Rotate around Y axis
        sun.transform.rotation = Quaternion.Euler(50f, sunAngle, 0f);

        // Update color
        sun.color = lightColor.Evaluate(time);

        // Update intensity
        sun.intensity = lightIntensity.Evaluate(time);

        skyboxInstance.SetColor("_Tint", skyColor.Evaluate(time));
        skyboxInstance.SetFloat("_Exposure", skyIntensity.Evaluate(time));
        DynamicGI.UpdateEnvironment();
    }
}
