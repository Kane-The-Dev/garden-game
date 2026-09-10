using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResearchCard : MonoBehaviour
{
    [SerializeField] string ID;
    [SerializeField] string myName, description;
    [SerializeField] ResearchType myType;
    [SerializeField] ModType modType;
    [SerializeField] float buffAmount;
    [SerializeField] int cost;

    [SerializeField] List<ResearchCard> prevCards;
    public List<ResearchCard> nextCards;
    public bool isUnlocked = false, isPurchased = false;
    
    [SerializeField] GameObject connector, lockIcon, purchaseIcon, details;
    [SerializeField] TextMeshProUGUI temporaryText, detailsText;
    ResearchCenter manager;

    // for editor
    void OnValidate()
    {
        gameObject.name = ID.ToString();
        temporaryText.text = ID.ToString();

        // float newX = Mathf.RoundToInt(transform.localPosition.x / 50) * 50;
        // float newY = Mathf.RoundToInt(transform.localPosition.y / 50) * 50;
        // transform.localPosition = new Vector3(newX, newY, 0);
    }

    void Awake() 
    {
        if (prevCards != null && prevCards.Count > 0)
        {
            bool anyPurchased = false;

            foreach (ResearchCard prevCard in prevCards)
            {
                if (prevCard == null) continue;

                if (prevCard.nextCards == null)
                    prevCard.nextCards = new List<ResearchCard>();

                if (!prevCard.nextCards.Contains(this))
                    prevCard.nextCards.Add(this);

                if (prevCard.isPurchased)
                    anyPurchased = true;

                if (connector != null)
                    CreateConnector(prevCard);
            }

            isUnlocked = anyPurchased;
        }
        else isUnlocked = true;
    }

    void CreateConnector(ResearchCard prevCard)
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
            rect.sizeDelta = new Vector2(dist, rect.sizeDelta.y);

        float angle = Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;
        line.transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Start()
    {
        manager = GameManager.instance.research;
        Refresh(manager.GetUpgrade(ID));
    }

    public void Refresh(Upgrade up) 
    {
        myName = up.name;
        description = up.description;

        myType = up.typeR;
        modType = up.typeM;

        buffAmount = up.amount;
        cost = up.cost;

        detailsText.text = myName + '\n' + description;

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

        if (nextCards != null)
        {
            foreach(ResearchCard card in nextCards) {
                if (card == null) continue;
                card.isUnlocked = true;
                if (card.lockIcon != null)
                    card.lockIcon.SetActive(false);
            }
        }
        
        isPurchased = true;
        purchaseIcon.SetActive(true);
        OpenDetails();

        switch (myType)
        {
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
            case ResearchType.fruitCount:
                manager.ApplyFruitCount(new Modifier(manager, modType, buffAmount, -1f));
                break;
            case ResearchType.windChance:
                manager.ApplyWindChance(new Modifier(manager, modType, buffAmount, -1f));
                break;
            case ResearchType.overtime:
                manager.ApplyOvertime(new Modifier(manager, modType, buffAmount, -1f));
                break;
            case ResearchType.betterTool:
                manager.ApplyBetterTool(new Modifier(manager, modType, buffAmount, -1f));
                break;
            case ResearchType.saleCount:
                manager.ApplySaleCount(new Modifier(manager, modType, buffAmount, -1f));
                break;
            case ResearchType.decorSale:
                manager.ApplyDecorSale(new Modifier(manager, modType, buffAmount, -1f));
                break;
            case ResearchType.shopDiscount:
                manager.ApplyShopDiscount(new Modifier(manager, modType, buffAmount, -1f));
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
