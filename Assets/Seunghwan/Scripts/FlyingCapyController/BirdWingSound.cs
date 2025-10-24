using System.Collections.Generic;
using UnityEngine;

public class BirdWingSound : MonoBehaviour
{
    [SerializeField] private AudioClip wingSound;
    [SerializeField] private AudioSource wingAudioSource;

    [SerializeField] private float pitchRandomDeviation = 0.1f;

    public bool BlockSound { get => blockSound; set => blockSound = value; }
    private bool blockSound = false;
    
    public void PlayWingFlapSound()
    {
        if (blockSound) return;
        
        if (wingSound != null)
        {
            float randomPitch = 1f + Random.Range(-pitchRandomDeviation, pitchRandomDeviation);
            wingAudioSource.pitch = randomPitch;
            wingAudioSource.PlayOneShot(wingSound);
        }
    }
}
