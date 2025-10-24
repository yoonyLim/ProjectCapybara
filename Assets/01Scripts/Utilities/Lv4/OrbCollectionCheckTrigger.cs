using System;
using System.Collections.Generic;
using Capybara;
using UnityEngine;
using UnityEngine.Events;

public class OrbCollectionCheckTrigger : MonoBehaviour
{
    [SerializeField] List<CollectGlowingOrb> collectGlowingOrbs;
    [SerializeField] AudioClip collectSound;

    private int collectedOrbNum = 0;
    private AudioSource audioSource;
    
    public UnityAction OnOrbsCollected;
    
    private void Start()
    {
        GameObject audioManager = GameObject.FindWithTag($"SFX");

        if (audioManager)
        {
            Debug.Log("yay sound!");
            audioSource = audioManager.GetComponent<AudioSource>();
        }
        
        foreach (var orb in collectGlowingOrbs)
        {
            orb.OnCollected += IncreaseCollectedOrbNum;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<CapybaraControllerSnow>().OrbCollectionFailed();
        }
    }

    private void IncreaseCollectedOrbNum()
    {
        collectedOrbNum++;
        audioSource.clip = collectSound;
        audioSource.Play();
        
        // play sound
        
        if (collectedOrbNum >= 3)
            OnOrbsCollected?.Invoke();
        
        Debug.Log(collectedOrbNum);
    }
}
