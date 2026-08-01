using UnityEngine;
using UnityEngine.EventSystems;

public class UIButton : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] int hoverID = 1, clickID = 0;
    AudioManager am;
    float hoverCooldown = 0.5f, timer;

    void Start()
    {
        am = AudioManager.instance;
    }

    void Update()
    {
        if (timer > 0f) timer -= Time.deltaTime;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnHover();
    }

    void OnHover()
    {
        if (timer <= 0f)
        {
            am.PlayUISoundEffect(hoverID);
            timer = hoverCooldown;
        }
    }

    public void OnClick()
    {
        am.PlayUISoundEffect(clickID);
    }
}