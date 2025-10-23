using Capybara;
using UnityEngine;

/// <summary>
/// [수정됨] InputReader의 'Interact' 이벤트를 구독하여 대화 테스트를 시작합니다.
/// </summary>
public class DialogueTester : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("CapybaraInputReader 에셋을 여기에 할당하세요.")]
    [SerializeField] private CapybaraInputReader inputReader;

    [Header("NPC To Test")]
    [Tooltip("테스트 시 대화를 시작할 NPC (Dialogue 컴포넌트가 붙어있는)")]
    [SerializeField] private Dialogue npcDialogueComponent;

    // KeyCode testKey 필드 제거

    private void OnEnable()
    {
        if (inputReader == null)
        {
            Debug.LogError("InputReader가 DialogueTester에 할당되지 않았습니다!", this);
            return;
        }

        // GamePlay 맵의 'InteractEvent'를 구독합니다.
        // 만약 InputReader의 이벤트 이름이 다르다면 (예: SubmitEvent_GamePlay) 변경해야 합니다.
        inputReader.InteractEvent += StartTestDialogue;
    }

    private void OnDisable()
    {
        if (inputReader != null)
        {
            inputReader.InteractEvent -= StartTestDialogue;
        }
    }

    /// <summary>
    /// InputReader의 Interact 이벤트가 호출되면 실행됩니다.
    /// </summary>
    private void StartTestDialogue()
    {
        // UIManager가 있고, 메뉴 UI가 닫혀 있으며, 다른 대화가 활성화되지 않았는지 확인
        if (UIManager.instance != null && !UIManager.instance.IsMenuUIOpen && !UIManager.instance.IsDialogueActive)
        {
            // 테스트할 NPC가 할당되었는지 확인
            if (npcDialogueComponent != null)
            {

                Debug.Log($"[InputReader.InteractEvent] {npcDialogueComponent.name}의 대화를 시작합니다.");

                // NPC의 Dialogue 컴포넌트에 직접 대화 시작을 명령
                npcDialogueComponent.StartDialogue();
            }
            else
            {
                Debug.LogWarning("[InputReader.InteractEvent] 테스트할 NPC가 할당되지 않았습니다.");
            }
        }
        else if (UIManager.instance == null)
        {
            Debug.LogError("UIManager 인스턴스를 찾을 수 없습니다.");
        }
        else
        {
            Debug.Log("[InputReader.InteractEvent] 이미 다른 UI가 열려있거나 대화 중입니다.");
        }
    }

    // Update() 메서드 제거
}