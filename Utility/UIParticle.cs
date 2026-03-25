using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIParticle : MonoBehaviour
{
    [SerializeField] bool type; // 0 = image, 1 = text
    [SerializeField] Image img;
    public TextMeshProUGUI text;
    public Vector2 velocity;
    public float gravity = -800f;
    public float lifetime = 1f;
    public float spinSpeed;
    public float flipSpeed;
    public RectTransform target;

    float timer;
    RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        rect.Rotate(0, 0, spinSpeed * Time.deltaTime);

        float h = Mathf.Abs(Mathf.Cos(Time.time * flipSpeed)) + 0.1f;
        rect.localScale = new Vector3(1.1f, h, 1.1f);

        Vector2 myPos = rect.anchoredPosition;

        if (target)
        {
            Vector2 targetPos = target.anchoredPosition;
            Vector2 toTarget = targetPos - myPos;

            float dist = toTarget.magnitude;
            Vector2 dir = toTarget.normalized;
            Vector2 desiredVelocity = dir * 2000f;
            velocity = Vector2.Lerp(velocity, desiredVelocity, 2f * Time.deltaTime);
            velocity = Vector2.ClampMagnitude(velocity, 2000f);

            if (dist < 30f)
            {
                AudioManager.instance.PlayUISoundEffect(3);
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        rect.anchoredPosition += velocity * Time.deltaTime;

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
