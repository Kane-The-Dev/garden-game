using System.Collections;
using UnityEngine;

public class Tornado : MonoBehaviour
{
    [SerializeField] ParticleSystemForceField psff;

    [Header("Wind Settings")]
    [SerializeField] float maxScale = 1f;
    [SerializeField] float maxStrength = 5f;
    [SerializeField] float windUpDuration = 1.5f;
    [SerializeField] float windDownDuration = 1f;

    Coroutine windCoroutine;

    void Awake()
    {
        if (!psff) psff = GetComponentInChildren<ParticleSystemForceField>();

        // Start invisible
        transform.localScale = Vector3.zero;
        if (psff) psff.gravity = 0f;
    }

    public void StartWind()
    {
        if (windCoroutine != null) StopCoroutine(windCoroutine);
        windCoroutine = StartCoroutine(ScaleWind(transform.localScale.x, maxScale, windUpDuration));
    }

    public void StopWind()
    {
        if (windCoroutine != null) StopCoroutine(windCoroutine);
        windCoroutine = StartCoroutine(ScaleWind(transform.localScale.x, 0f, windDownDuration));
    }

    IEnumerator ScaleWind(float fromScale, float toScale, float duration)
    {
        float fromStrength = psff ? psff.gravity.constant : 0f;
        float toStrength   = Mathf.Approximately(toScale, 0f) ? 0f : maxStrength;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float pct = Mathf.Clamp01(t / duration);

            transform.localScale = Vector3.one * Mathf.Lerp(fromScale, toScale, pct);
            if (psff) psff.gravity = Mathf.Lerp(fromStrength, toStrength, pct);

            yield return null;
        }

        // Snap to final values
        transform.localScale = Vector3.one * toScale;
        if (psff) psff.gravity = toStrength;

        windCoroutine = null;
    }
}
