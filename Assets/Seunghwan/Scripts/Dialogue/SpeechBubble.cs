using System;
using UnityEngine;

public class SpeechBubble : MonoBehaviour
{
    private Camera mainCamera;
    
    [SerializeField] private Canvas speechBubbleCanvas;

    private void OnEnable()
    {
        // InteractionComponent.OnDialogStart += DisableSpeechBubble();
        // InteractionComponent.OnDialogEnd += EnableSpeechBubble;
    }

    private void OnDisable()
    {
        // InteractionComponent.OnDialogStart -= DisableSpeechBubble();
        // InteractionComponent.OnDialogEnd -= EnableSpeechBubble;
    }

    private void Awake()
    {
        mainCamera = Camera.main;
    }
    
    void Update()
    {
        if (speechBubbleCanvas.enabled)
        {
            Quaternion cameraRotation = mainCamera.transform.rotation;
            speechBubbleCanvas.transform.rotation = cameraRotation;
        }
    }

    public void EnableSpeechBubble()
    {
        speechBubbleCanvas.enabled = true;
    }

    public void DisableSpeechBubble()
    {
        speechBubbleCanvas.enabled = false;
    }
}
