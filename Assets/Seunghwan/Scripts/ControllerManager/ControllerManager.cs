using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class ControllerManager : MonoBehaviour
{
    [SerializeField] private PlayerController walkController;
    [SerializeField] private CinemachineCamera walkCamera;
    [SerializeField] private FlyModeController flyController;
    [SerializeField] private CinemachineCamera flyCamera;
    

    [SerializeField] private Dialogue birdDialogue;

    [SerializeField] private Image blackImage;

    private void OnEnable()
    {
        birdDialogue.OnDialogueEnd += OnBirdDialogueEnd;
    }

    private void OnDisable()
    {
        birdDialogue.OnDialogueEnd -= OnBirdDialogueEnd;
    }

    private void OnBirdDialogueEnd()
    {
        StartCoroutine(FadeOutIn());
    }

    IEnumerator FadeOutIn()
    {
        Color transparentColor = new Color(0, 0, 0, 0);
        Color blackColor = new Color(0, 0, 0, 1f);
        float fadeOutElapsedTime = 0f;
        while (fadeOutElapsedTime < 0.5f)
        {
            fadeOutElapsedTime += Time.deltaTime;
            blackImage.color = Color.Lerp(transparentColor, blackColor, fadeOutElapsedTime / 1f);
            yield return null;
        }
        blackImage.color = blackColor;
        
        Time.timeScale = 0f;
        
        yield return new WaitForSeconds(0.5f);

        
        walkController.gameObject.SetActive(false);
        walkCamera.enabled = false;
        flyCamera.enabled = true;
        flyController.gameObject.SetActive(true);
        
        
        float fadeInElapsedTime = 0f;
        while (fadeInElapsedTime < 0.5f)
        {
            fadeInElapsedTime += Time.deltaTime;
            blackImage.color = Color.Lerp(blackColor, transparentColor, fadeInElapsedTime / 1f);
            yield return null;
        }
        blackImage.color = transparentColor;
    }
}
