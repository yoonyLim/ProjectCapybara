using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldCameraManager : MonoBehaviour
{
    public static WorldCameraManager instance;

    public Dictionary<string, DynamicCamera> cameras = new Dictionary<string, DynamicCamera>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        foreach (var kv in cameras)
        {
            kv.Value.cinemachineCamera.Priority = 0;
        }
    }

    public void RegisterCamera(DynamicCamera newCamera)
    {
        cameras[newCamera.cameraData.cameraName] = newCamera;
    }

    public void MakeLive(DynamicCamera targetCamera)
    {
        cameras[targetCamera.cameraData.cameraName].cinemachineCamera.Priority = 2;
    }

    public void MakeStandby(DynamicCamera targetCamera)
    {
        cameras[targetCamera.cameraData.cameraName].cinemachineCamera.Priority = 0;
    }
}
