using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindGenerator : MonoBehaviour
{
    public Stat harvestChance = new Stat(0f);

    void Start() 
    {
        StartCoroutine(StartWind());
    }

    IEnumerator StartWind()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);

            if (Random.Range(0f, 1f) < harvestChance.Value)
            {
                Growable[] growables = FindObjectsOfType<Growable>();
                List<Growable> candidates = new List<Growable>();

                foreach (Growable g in growables)
                {
                    if (g != null && !g.isProduct && !g.isOven && g.ripeFruitCount > 0)
                        candidates.Add(g);
                }

                if (candidates.Count > 0)
                {
                    Growable selected = candidates[Random.Range(0, candidates.Count)];
                    selected.HarvestFruit(1);
                    selected.Shake(4f);
                    Debug.Log(selected.name + " harvested by wind!");
                }
            }
        }
    }
}
