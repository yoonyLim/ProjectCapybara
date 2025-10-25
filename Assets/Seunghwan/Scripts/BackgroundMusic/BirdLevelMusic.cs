using System;
using System.Collections;
using UnityEngine;

public class BirdLevelMusic : MonoBehaviour
{
    public static BirdLevelMusic instance;

    public AudioSource firstPartMusic;
    public AudioSource secondPartMusic;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayFirstPartMusic()
    {
        firstPartMusic.Play();
    }

    public void PlaySecondPartMusic()
    {
        secondPartMusic.Play();
        StartCoroutine(FadeTransition());
    }

    IEnumerator FadeTransition()
    {
        float elapsedTime = 0;
        while (elapsedTime < 2f)
        {
            elapsedTime += Time.deltaTime;

            if (firstPartMusic.isPlaying)
            {
                firstPartMusic.volume = Mathf.Lerp(1f, 0f, elapsedTime / 2f);
            }
            
            secondPartMusic.volume = Mathf.Lerp(0f, 1f, elapsedTime / 2f);
            
            yield return null;
        }
        firstPartMusic.Stop();
        secondPartMusic.volume = 1f;
    }
}
