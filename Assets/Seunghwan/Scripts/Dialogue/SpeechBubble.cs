using System;
using UnityEngine;

public class SpeechBubble : MonoBehaviour
{
    private Camera mainCamera;
    
    private void Awake()
    {
        mainCamera = Camera.main;
    }

    
    void Update()
    {
        // Maybe optimize this behavior to be called once upon initialization?..(should consider camera movement state)
        transform.LookAt(mainCamera.transform);
    }
}
