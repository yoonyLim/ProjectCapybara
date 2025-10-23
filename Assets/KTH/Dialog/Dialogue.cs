using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// [로직 전용] NPC에 부착되어 대화의 '흐름' (어떤 노드인지)만 관리합니다.
/// 실제 표현(UI, 오디오)은 DialogUI.cs에 위임합니다.
/// </summary>
public class Dialogue : MonoBehaviour, IInteractable // IInteractable 인터페이스를 구현하고 있는지 확인하세요.
{
    public enum DialogueState
    {
        Inactive,
        Normal
    }

    public event Action OnDialogueStart;
    public event Action OnDialogueEnd;
    public event Action OnDialogueAdvance;

    [Header("NPC Info")] // [추가됨]
    [Tooltip("이 NPC의 이름입니다. DialogUI에 표시됩니다.")]
    [SerializeField] private string npcName; // [추가됨]

    [Header("Dialogue Content")]
    [Tooltip("이 NPC가 시작할 첫 번째 대화 노드(DialogueNode)입니다.")]
    [SerializeField]
    private DialogueNode startingNode;

    private DialogueNode currentNode;
    [SerializeField]
    public DialogueState currentState;

    public Coroutine currentDialogueCoroutine { get; private set; }

    private void Awake()
    {
        currentState = DialogueState.Inactive;
    }

    // InteractionComponent가 호출할 수 있도록 IInteractable 인터페이스를 구현합니다.
    public void Interact()
    {
        StartDialogue();
    }

    /// <summary>
    /// 대화를 시작합니다. (InteractionComponent 또는 DialogueTester에서 호출)
    /// </summary>
    public void StartDialogue()
    {
        // 1. UIManager에게 대화 시작을 알립니다.
        if (UIManager.instance != null)
        {
            UIManager.instance.StartDialogue(this);
        }

        // [추가됨] DialogUI에 NPC 이름 전달
        if (DialogUI.Instance != null)
        {
            DialogUI.Instance.SetNpcName(npcName);
        }
        else
        {
            Debug.LogWarning("DialogUI 인스턴스를 찾을 수 없어 이름을 설정할 수 없습니다.");
        }

        OnDialogueStart?.Invoke();

        if (startingNode == null)
        {
            Debug.LogError($"{gameObject.name}에 startingNode가 할당되지 않았습니다!");
            StartCoroutine(EndInteractionCoroutine());
            return;
        }

        currentNode = startingNode;
        currentState = DialogueState.Normal;

        currentDialogueCoroutine = StartCoroutine(ProcessDialogue());
    }

    /// <summary>
    /// 현재 노드를 처리하고, DialogUI에 텍스트 표시를 '요청'합니다.
    /// </summary>
    public IEnumerator ProcessDialogue()
    {
        if (currentNode)
        {
            OnDialogueAdvance?.Invoke();
            currentState = DialogueState.Normal;

            if (DialogUI.Instance != null)
            {
                yield return DialogUI.Instance.ShowDialog(currentNode.DialogueText);
            }
            else
            {
                Debug.LogError("DialogUI 인스턴스를 찾을 수 없습니다!");
                yield return null;
            }

            currentNode = currentNode.NextNode;
        }
        else
        {
            currentState = DialogueState.Inactive;
            StartCoroutine(EndInteractionCoroutine());
            OnDialogueEnd?.Invoke();
        }

        currentDialogueCoroutine = null;
    }

    /// <summary>
    /// UIManager가 '다음' 입력을 받았을 때 호출할 공용 함수입니다.
    /// </summary>
    public void AdvanceDialogue()
    {
        if (currentState == DialogueState.Normal && currentDialogueCoroutine == null)
        {
            currentDialogueCoroutine = StartCoroutine(ProcessDialogue());
        }
    }

    /// <summary>
    /// 대화가 완전히 종료될 때 UIManager에게 상태 복구를 요청합니다.
    /// </summary>
    private IEnumerator EndInteractionCoroutine()
    {
        if (UIManager.instance != null)
        {
            UIManager.instance.EndDialogue();
        }

        yield return new WaitForEndOfFrame();
        InteractionComponent.EndInteraction();
    }
}