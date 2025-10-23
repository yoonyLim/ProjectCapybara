using System;
using System.Collections.Generic;
using UnityEngine;

public class OrbCollectionCheckTrigger : MonoBehaviour
{
    [SerializeField] List<CollectGlowingOrb> collectGlowingOrbs;

    private int collectedOrbNum = 0;
    
    private void Start()
    {
        foreach (var orb in collectGlowingOrbs)
        {
            orb.OnCollected += IncreaseCollectedOrbNum;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GetComponent<Collider>().enabled = false;
        }
    }

    private void IncreaseCollectedOrbNum()
    {
        collectedOrbNum++;
        Debug.Log(collectedOrbNum);
    }
}
