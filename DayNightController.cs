using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightController : MonoBehaviour
{
    public Light sun;

    [Header("Rotation")]
    public float dayLength = 60f; // seconds for full cycle
    public float startTime = 0f;  // 0-1

    [Header("Light Settings")]
    public Gradient lightColor;
    public Gradient skyColor;
    public AnimationCurve lightIntensity, skyIntensity;

    Material skyboxInstance;

    float time;

    void Start()
    {
        time = startTime;
        skyboxInstance = new Material(RenderSettings.skybox);
        RenderSettings.skybox = skyboxInstance;
    }

    void Update()
    {
        time += Time.deltaTime / dayLength;
        time %= 1f;

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
