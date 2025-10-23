using System;
using UnityEngine;

public class InteractButtonCanvas : MonoBehaviour
{
    private Camera mainCamera;
    
    private void Awake()
    {
        mainCamera = Camera.main;
    }


    private void Update()
    {
        Debug.Log("앙기모띠앙기모띠");
    }

    void LateUpdate()
    {
        // Maybe optimize this behavior to be called once upon initialization? (If the camera does not move during dialogue)
        Quaternion cameraRotation = mainCamera.transform.rotation;
        transform.rotation = cameraRotation;
    }
}
