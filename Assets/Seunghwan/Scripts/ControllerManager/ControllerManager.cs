using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class ControllerManager : MonoBehaviour
{
    [SerializeField] private PlayerStateMachine walkController;
    [SerializeField] private CinemachineCamera walkCamera;
    [SerializeField] private FlyModeController flyController;
    [SerializeField] private CinemachineCamera flyCamera;

    [SerializeField] private Image blackImage;

    private CinemachineBrain cinemachineBrain;

    private void Start()
    {
        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        UIManager.instance.OnCloseLevelTutorial += HandleCloseLevelTutorial;
    }

    private void OnDisable()
    {
        UIManager.instance.OnCloseLevelTutorial -= HandleCloseLevelTutorial;
    }

    private void HandleCloseLevelTutorial()
    {
        StartCoroutine(FadeInOut());
    }

    IEnumerator FadeInOut()
    {
        
        
        Color transparentColor = new Color(0, 0, 0, 0);
        Color blackColor = new Color(0, 0, 0, 1f);
        float fadeOutElapsedTime = 0f;
        while (fadeOutElapsedTime < 0.75f)
        {
            fadeOutElapsedTime += Time.deltaTime;
            blackImage.color = Color.Lerp(transparentColor, blackColor, fadeOutElapsedTime / 1f);
            yield return null;
        }
        blackImage.color = blackColor;

        flyCamera.Priority = 40;
        yield return new WaitForSeconds(1f);
        
        walkController.gameObject.SetActive(false);
        flyController.gameObject.SetActive(true);
        
        
        float fadeInElapsedTime = 0f;
        while (fadeInElapsedTime < 0.75f)
        {
            fadeInElapsedTime += Time.deltaTime;
            blackImage.color = Color.Lerp(blackColor, transparentColor, fadeInElapsedTime / 1f);
            yield return null;
        }
        blackImage.color = transparentColor;
    }
    
}
