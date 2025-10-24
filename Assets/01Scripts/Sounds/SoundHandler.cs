using System.Collections;
using UnityEngine;

public class SoundHandler : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    public void FadeOutSong(float fadeTime)
    {
        StartCoroutine(FadeOut(fadeTime));
    }
    
    public void FadeInSong(float fadeTime)
    {
        StartCoroutine(FadeIn(fadeTime, 0.5f));
    }

    public void SwitchSong(AudioClip clip)
    {
        audioSource.clip = clip;
    }

    IEnumerator FadeOut(float fadeTime)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeTime);
            
            yield return null;
        }
        
        audioSource.volume = 0f;
        
        audioSource.Stop();
    }
    
    IEnumerator FadeIn(float fadeTime, float volume)
    {
        audioSource.Play();

        audioSource.volume = 0f;
        float startVolume = 0f;
        float timer = 0f;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            
            audioSource.volume = Mathf.Lerp(startVolume, volume, timer / fadeTime);
            
            yield return null;
        }
        
        audioSource.volume = volume;
    }
}
