using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudioSourceHolder : MonoBehaviour
{
    public Dictionary<string, AudioSource> audioSources = new Dictionary<string, AudioSource>();

    private void Awake()
    {
        RegisterAudioSources();
    }

    private void RegisterAudioSources()
    {
        AudioSource[] audioSourceList = GetComponentsInChildren<AudioSource>();
        foreach (AudioSource audioSource in audioSourceList)
        {
            Debug.Log(audioSource.gameObject.name);
            audioSources.Add(audioSource.gameObject.name, audioSource);
        }
    }
}
