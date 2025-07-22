using System;
using UnityEngine;

public class SpeechBubble : MonoBehaviour
{
    private Camera mainCamera;
    
    private void Awake()
    {
        mainCamera = Camera.main;
    }

    
    void LateUpdate()
    {
        // Maybe optimize this behavior to be called once upon initialization? (If the camera does not move during dialogue)
        Quaternion cameraRotation = mainCamera.transform.rotation;
        transform.rotation = cameraRotation;
    }
}
