using System;
using UnityEngine;

public class HeadbuttVFXPlayer : MonoBehaviour
{
    private ParticleSystem headbuttVFX;

    private void Awake()
    {
        headbuttVFX = GetComponent<ParticleSystem>();
    }

    public void PlayHeadbuttVFX()
    {
        headbuttVFX.Play();
    }
}
