using System.Collections.Generic;
using UnityEngine;

public class BirdWingSound : MonoBehaviour
{
    [SerializeField] private AudioClip wingSound;
    [SerializeField] private AudioSource wingAudioSource;

    [SerializeField] private float pitchRandomDeviation = 0.1f;
    
    
    public void PlayWingFlapSound()
    {
        if (wingSound != null)
        {
            float randomPitch = 1f + Random.Range(-pitchRandomDeviation, pitchRandomDeviation);
            wingAudioSource.pitch = randomPitch;
            wingAudioSource.PlayOneShot(wingSound);
        }
    }
}
