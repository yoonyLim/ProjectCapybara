using System;
using UnityEngine;

public class PianoTracker : MonoBehaviour
{
    private int collectedKeyCount = 0;

    private int targetKeyCount = 5;
    
    private static bool canFixPiano = false;

    private readonly string collectSoundName = "PianoKey";

    public int GetCollectedKeyCount() => collectedKeyCount;
    public static bool GetCanFixPiano() => canFixPiano;

    public void OnKeyCollected()
    {
        collectedKeyCount++;
        if (collectedKeyCount == targetKeyCount)
        {
            canFixPiano = true;
        }
        
        SoundManager.instance.PlaySFX(collectSoundName);
        
    }
    
}
