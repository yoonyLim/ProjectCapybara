using Moko;
using Unity.Cinemachine;
using UnityEngine;

public class DynamicCamera : MonoBehaviour
{
    public CameraData cameraData;
    public CinemachineCamera cinemachineCamera;

    private void Awake()
    {
        if (cameraData == null) DebugExtension.Log(this, "Camera data is null", "yellow");
        
        WorldCameraManager.instance.RegisterCamera(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            WorldCameraManager.instance.MakeLive(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            WorldCameraManager.instance.MakeStandby(this);
        }
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.0f, 1f, 0.05f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);

        Gizmos.color = new Color(1f, 0.0f, 1f, 1f);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
#endif
}
