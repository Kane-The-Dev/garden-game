using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FenceManager : MonoBehaviour
{
    GameManager gm;
    public int myLevel;
    public GameObject[] fences;
    public Animator animator;
    [SerializeField] AdvancedAudioSource myAAS;
    [SerializeField] AudioClip[] woodBursts;
    [SerializeField] AudioClip construct;

    void Start()
    {
        gm = GameManager.instance;
        myLevel = 1;
    }

    public void SetFence(int level)
    {
        if (level == 0) return;

        for(int i = 0; i < fences.Length; i++)
        {
            fences[i].SetActive(level > 0 && i == level - 1);
        }
        myLevel = level;
    }

    public void UpgradeFence()
    {
        myLevel++;

        gm.UIAnimator.SetTrigger("closeshop");
        gm.ChangeMode(2);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        animator.SetTrigger("Upgrade!");
        Invoke("FinishUpgrade", 8f);
    }

    void FinishUpgrade()
    {
        gm.ChangeMode(0);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void WoodBurst()
    {
        myAAS.PlayOneShot(woodBursts[Random.Range(0, woodBursts.Length)], 0.2f, true);
    }

    public void Contruct()
    {
        myAAS.Play(construct, 0.8f, false, 0.5f);
    }

    public void StopContruct()
    {
        myAAS.Stop(0.5f);
    }
}
