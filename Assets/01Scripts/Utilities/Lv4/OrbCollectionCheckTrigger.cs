using System;
using System.Collections.Generic;
using Capybara;
using UnityEngine;
using UnityEngine.Events;

public class OrbCollectionCheckTrigger : MonoBehaviour
{
    [SerializeField] List<CollectGlowingOrb> collectGlowingOrbs;

    private int collectedOrbNum = 0;
    
    public UnityAction OnOrbsCollected;
    
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
            /*if (collectedOrbNum >= 3)
            {
                Debug.Log("success");
                CapybaraControllerSnow Capy = other.GetComponent<CapybaraControllerSnow>();
                Capy.OrbsCollectedSuccessfully();
            }
            else
            {
                Debug.Log("fail");
                
            }*/
            
            other.GetComponent<CapybaraControllerSnow>().OrbCollectionFailed();
        }
    }

    private void IncreaseCollectedOrbNum()
    {
        collectedOrbNum++;
        
        if (collectedOrbNum >= 3)
            OnOrbsCollected?.Invoke();
        
        Debug.Log(collectedOrbNum);
    }
}
