using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FenceManager : MonoBehaviour
{
    GameManager gm;
    public Animator animator;
    [SerializeField] AdvancedAudioSource myAAS;
    [SerializeField] AudioClip[] woodBursts;
    [SerializeField] AudioClip construct;

    void Start()
    {
        gm = GameManager.instance;
    }

    public void UpgradeFence()
    {
        animator.SetTrigger("Upgrade!");
        gm.sm.shopPanel.gameObject.SetActive(false);
        gm.ChangeMode(2);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
