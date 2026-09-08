using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResearchCard : MonoBehaviour
{
    [SerializeField] ResearchType myType;
    [SerializeField] ModType modType;
    [SerializeField] float buffAmount;
    [SerializeField] ResearchCard prevCard;
    public List<ResearchCard> nextCards;
    public bool isUnlocked = false, isPurchased = false;
    [SerializeField] int price;
    [SerializeField] GameObject connector, lockIcon, purchaseIcon, details;
    [SerializeField] TextMeshProUGUI myText;
    ResearchCenter manager;

    // for editor
    void OnValidate()
    {
        gameObject.name = myType.ToString();
        string sign = buffAmount > 0 ? "+" : "";
        if (myText == null) return;
        if (modType == ModType.Flat) myText.text = $"{myType} {sign}{Mathf.RoundToInt(buffAmount)}";
        else if (modType == ModType.PercentAdd) myText.text = $"{myType} {sign}{Mathf.RoundToInt(buffAmount * 100)}%";
    }

    void Awake() 
    {
        if (prevCard) 
        {
            prevCard.nextCards.Add(this);
            isUnlocked = prevCard.isPurchased;
        }
        else isUnlocked = true;

        if (connector != null && prevCard != null)
        {
            GameObject line = Instantiate(connector, transform.parent);
            line.transform.SetAsFirstSibling();

            Vector3 a = prevCard.transform.position;
            Vector3 b = transform.position;
            Vector3 mid = (a + b) * 0.5f;

            line.transform.position = mid;

            float dist = Vector3.Distance(a, b);
            RectTransform rect = line.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = new Vector2(dist, rect.sizeDelta.y);
            }

            float angle = Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;
            line.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    void Start()
    {
        manager = GameManager.instance.research;
        Refresh();
    }

    public void Refresh() 
    {
        lockIcon.SetActive(!isUnlocked);
        purchaseIcon.SetActive(isPurchased);
    }

    public void OpenDetails()
    {
        if (!details.activeSelf) 
        {
            details.SetActive(true);
            details.transform.SetParent(transform.parent);
            details.transform.SetAsLastSibling();
        }
        else 
        {
            details.SetActive(false);
            details.transform.SetParent(transform);
            details.transform.SetAsLastSibling();
        }
    }

    public void OnUnlock()
    {
        if (!isUnlocked || isPurchased) return;

        foreach(ResearchCard card in nextCards) {
            card.isUnlocked = true;
            card.lockIcon.SetActive(false);
        }
        
        isPurchased = true;
        purchaseIcon.SetActive(true);
        OpenDetails();

        switch (myType)
        {
            case ResearchType.growth_G:
                manager.ApplyGeneralGrowth(new Modifier(manager, modType, buffAmount, -1f));
                break;
            case ResearchType.growth_P:
                manager.ApplyPlantGrowth(new Modifier(manager, modType, buffAmount, -1f));
                break;
            case ResearchType.growth_O:
                manager.ApplyOvenGrowth(new Modifier(manager, modType, buffAmount, -1f));
                break;
            case ResearchType.profit_G:
                manager.ApplyGeneralProfit(new Modifier(manager, modType, buffAmount, -1f));
                break;
            case ResearchType.profit_P:
                manager.ApplyPlantProfit(new Modifier(manager, modType, buffAmount, -1f));
                break;
            case ResearchType.profit_O:
                manager.ApplyOvenProfit(new Modifier(manager, modType, buffAmount, -1f));
                break;
            case ResearchType.goldFruit:
                manager.ApplyGoldenFruit(new Modifier(manager, modType, buffAmount, -1f));
                break;
            case ResearchType.windChance:
                manager.ApplyWindChance(new Modifier(manager, modType, buffAmount, -1f));
                break;
            case ResearchType.fruitCount:
                manager.ApplyFruitCount(new Modifier(manager, modType, -buffAmount, -1f));
                break;
            case ResearchType.truckWeight:
                manager.ApplyTruckWeight(new Modifier(manager, modType, buffAmount, -1f));
                break;
            case ResearchType.truckCooldown:
                manager.ApplyTruckCooldown(new Modifier(manager, modType, buffAmount, -1f));
                break;
            case ResearchType.truckCount:
                manager.ApplyTruckCount(new Modifier(manager, modType, buffAmount, -1f));
                break;
        }
    }
}
