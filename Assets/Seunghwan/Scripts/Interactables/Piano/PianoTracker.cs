using System;
using UnityEngine;

public class PianoTracker : MonoBehaviour
{
    private int collectedKeyCount = 0;

    private int targetKeyCount = 5;
    
    [SerializeField] 
    private AudioClip collectSound;
    
    private AudioSource audioSource;

    private static bool canFixPiano = false;
    

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public int GetCollectedKeyCount() => collectedKeyCount;
    public static bool GetCanFixPiano() => canFixPiano;

    public void OnKeyCollected()
    {
        collectedKeyCount++;
        if (collectedKeyCount == targetKeyCount)
        {
            canFixPiano = true;
        }
        
        audioSource.PlayOneShot(collectSound);
        
    }
    
}
