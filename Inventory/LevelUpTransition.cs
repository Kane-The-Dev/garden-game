using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class LevelUpTransition : MonoBehaviour
{
    [SerializeField] Inventory inventory;
    [SerializeField] Animator anim;

    [SerializeField] Image badge;
    [SerializeField] Sprite[] allBadges;

    [SerializeField] TextMeshProUGUI levelDisplay;
    [SerializeField] Color[] badgeColors;

    [SerializeField] GameObject slot;
    [SerializeField] Transform rewardsList;
    
    [SerializeField] GameObject claimButton;
    [SerializeField] UIParticleSystem coinBurst;

    bool rewardsCollected = true;
    int myLevel;
    GameManager gm;
    List<(string name, int quantity)> currentRewards;

    void Start()
    {
        if (!anim) anim = GetComponent<Animator>();
        gm = GameManager.instance;
        if (!inventory) inventory = gm.inventory;
    }

    public void LevelUp(int level)
    {
        if (!rewardsCollected) {
            ClaimRewards();
            myLevel = level;
            Invoke("Activate", 0.2f);
        }
        else {
            myLevel = level;
            Activate();
        }
    }

    void Activate()
    {
        rewardsCollected = false;
        gm.cam.movable = false;

        int i = Mathf.Min(myLevel / 12, allBadges.Length - 1);
        badge.sprite = allBadges[i];
        levelDisplay.color = badgeColors[i];

        claimButton.GetComponent<CanvasGroup>().alpha = 0;
        claimButton.GetComponent<Button>().interactable = false;
        anim.SetTrigger("up");

        if (rewardsList != null)
        {
            foreach (Transform child in rewardsList)
                Destroy(child.gameObject);
        }

        currentRewards = ReadFile.GetLevelUpRewards(myLevel);

        StartCoroutine(LevelCountUp(myLevel));
    }

    IEnumerator LevelCountUp(int level)
    {
        int step = 1 + level / 20;
        for (int i = 1; i < level; i += step)
        {
            levelDisplay.text = i.ToString();
            yield return new WaitForSeconds(0.05f);
        }
        levelDisplay.text = level.ToString();

        yield return new WaitForSeconds(0.15f);

        if (currentRewards != null)
        {
            foreach (var reward in currentRewards)
            {
                GameObject newSlotGo = Instantiate(slot, rewardsList);
                Slot newSlot = newSlotGo.GetComponent<Slot>();
                if (newSlot != null)
                {
                    newSlot.itemName = reward.name;
                    Sprite icon = ReadFile.LoadIconSprite(reward.name);
                    newSlot.SetIcon(icon);
                    newSlot.SetQuantity(reward.quantity);
                    newSlot.Initialize(-1, inventory.myDisplay, null);
                }
                yield return new WaitForSeconds(0.15f);
            }
        }

        CanvasGroup cg = claimButton.GetComponent<CanvasGroup>();

        float t = 0f;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(t / 0.15f);
            yield return null;
        }
        claimButton.GetComponent<Button>().interactable = true;
    }

    public void ClaimRewards()
    {
        gm.cam.movable = true;
        rewardsCollected = true;
        anim.SetTrigger("claim");
        
        if (currentRewards != null)
        {
            foreach (var reward in currentRewards)
            {
                if (reward.name.Equals("money", System.StringComparison.OrdinalIgnoreCase))
                {
                    int bonus = reward.quantity;
                    inventory.coin += bonus;
                    coinBurst.minCount = Mathf.Min(1 + bonus / 20, 30);
                    coinBurst.maxCount = Mathf.Min(1 + bonus / 20, 30);
                    coinBurst.Emission(0.05f);
                }
                else if (reward.name.Equals("research credit", System.StringComparison.OrdinalIgnoreCase))
                {
                    gm.research.researchCredit++;
                }
                else
                {
                    int n = reward.quantity;
                    ItemType type = inventory.GetItemType(reward.name);
                    if (n > 0) inventory.AddItemQuantity(reward.name, n, type);
                }
            }
        }

        Debug.Log("Claimed rewards for level " + myLevel);
    }
}
