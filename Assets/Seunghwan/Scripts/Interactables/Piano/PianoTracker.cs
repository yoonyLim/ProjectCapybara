using System;
using UnityEngine;

public class PianoTracker : MonoBehaviour
{
    private int collectedKeyCount = 0;

    private int targetKeyCount = 5;
    
    [SerializeField] 
    private AudioClip collectSound;
    
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public int GetCollectedKeyCount()
    {
        return collectedKeyCount;
    }

    public void OnKeyCollected()
    {
        collectedKeyCount++;
        if (collectedKeyCount == targetKeyCount)
        {
            // TODO Quest Completed Logic
        }
        
        audioSource.PlayOneShot(collectSound);
        
        
    }
    
}
