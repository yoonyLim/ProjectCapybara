using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BirdHitSound : MonoBehaviour
{
    [SerializeField] private List<AudioClip> hitSounds;
    private AudioSource hitAudioSource;

    private void Awake()
    {
        hitAudioSource = GetComponent<AudioSource>();
    }

    public void PlayHitSound()
    {
        if (hitSounds.Count == 0) return;
        
        AudioClip sound = hitSounds[Random.Range(0, hitSounds.Count)];
        hitAudioSource.PlayOneShot(sound);
    }
}
