using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;



// Cinemachine State와 같은 이름을 사용할 것
public enum CameraViewMode
{
    PlayerCamera,
    PlayerDialogueCamera,
}

/// <summary>
/// Class that Exposes Camera Switch Methods
/// </summary>
public class PlayerCameraController : MonoBehaviour
{
    private Animator animator;
    private GameObject player;
    private NPCDetector npcDetector;    
    [SerializeField] private CinemachineTargetGroup targetGroup;

    // 현재 카메라 모드
    public CameraViewMode CurrentCameraMode { get; private set; }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
        CurrentCameraMode = CameraViewMode.PlayerCamera;
        npcDetector = player.GetComponent<NPCDetector>();
    }

    // mode가 바뀌면, CineMachine Camera Transition이 이루어짐
    public void SwitchCameraMode(CameraViewMode mode)
    {
        CurrentCameraMode = mode;
        animator.Play($"{mode}");
    }





    // -----------------------test code-----------------------
    #if UNITY_EDITOR
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (CurrentCameraMode == CameraViewMode.PlayerCamera)
            {
                if (npcDetector.currentDetectedNPC)
                {
                    targetGroup.AddMember(npcDetector.currentDetectedNPC.transform, 1, 1);
                    SwitchCameraMode(CameraViewMode.PlayerDialogueCamera);
                }
            }
            else
            {
                targetGroup.RemoveMember(npcDetector.currentDetectedNPC.transform);
                SwitchCameraMode(CameraViewMode.PlayerCamera);
            }
        }
    }
    #endif
    // -----------------------test code-----------------------
}
