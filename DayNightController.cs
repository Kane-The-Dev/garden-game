using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DayNightController : MonoBehaviour
{
    public Light sun;

    [Header("Visualization")]
    [SerializeField] TextMeshProUGUI dayDisplay;
    [SerializeField] RectTransform clock;

    [Header("Rotation")]
    public float dayLength = 60f; // seconds for full cycle
    public float startTime = 0f;  // 0-1

    [Header("Light Settings")]
    public Gradient lightColor;
    public Gradient skyColor;
    public AnimationCurve lightIntensity, skyIntensity;

    Material skyboxInstance;

    float time;
    int dayCount;

    void Start()
    {
        dayCount = 0;
        time = startTime;
        skyboxInstance = new Material(RenderSettings.skybox);
        RenderSettings.skybox = skyboxInstance;
    }

    void Update()
    {
        time += Time.deltaTime / dayLength;
        if (time >= 1f) dayCount += 1;
        time %= 1f;

        dayDisplay.text = dayCount.ToString();
        clock.rotation = Quaternion.Euler(0f, 0f, 360f * time);

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
