using System;
using UnityEngine;

public class SoundEventManager : MonoBehaviour
{
    public static SoundEventManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    
}
