using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIParticle : MonoBehaviour
{
    public Vector2 velocity;
    public float gravity = -800f;
    public float lifetime = 1f;
    public float spinSpeed;
    public float flipSpeed;

    float timer;
    RectTransform rect;
    Image img;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        img = GetComponent<Image>();
        flipSpeed = Random.Range(5f, 10f);
    }

    void Update()
    {
        timer += Time.deltaTime;

        velocity.y += gravity * Time.deltaTime;
        rect.anchoredPosition += velocity * Time.deltaTime;

        rect.Rotate(0, 0, spinSpeed * Time.deltaTime);

        float h = Mathf.Abs(Mathf.Sin(Time.time * flipSpeed)) + 0.1f;
        rect.localScale = new Vector3(1.1f, h, 1.1f);

        // fade
        Color c = img.color;
        c.a = 1f - timer / lifetime;
        img.color = c;

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}
