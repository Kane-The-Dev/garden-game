using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIParticle : MonoBehaviour
{
    [SerializeField] bool type; // 0 = image, 1 = text
    [SerializeField] Image img;
    [SerializeField] TextMeshProUGUI text;
    public Vector2 velocity;
    public float gravity = -800f;
    public float lifetime = 1f;
    public float spinSpeed;
    public float flipSpeed;

    float timer;
    RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        velocity.y += gravity * Time.deltaTime;
        rect.anchoredPosition += velocity * Time.deltaTime;

        rect.Rotate(0, 0, spinSpeed * Time.deltaTime);

        float h = Mathf.Abs(Mathf.Cos(Time.time * flipSpeed)) + 0.1f;
        rect.localScale = new Vector3(1.1f, h, 1.1f);

        // fade
        if (type == false)
        {
            Color c = img.color;
            c.a = 1f - timer / lifetime;
            img.color = c;
        }
        else
        {
            Color c = text.color;
            c.a = 1f - timer / lifetime;
            text.color = c;
        } 

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}
