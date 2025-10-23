using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// [로직 전용] NPC에 부착되어 대화의 '흐름' (어떤 노드인지)만 관리합니다.
/// 실제 표현(UI, 오디오)은 DialogUI.cs에 위임합니다.
/// </summary>
public class Dialogue : MonoBehaviour, IInteractable
{
    public enum DialogueState
    {
        Inactive,
        Normal
    }

    public event Action OnDialogueStart;
    public event Action OnDialogueEnd;
    public event Action OnDialogueAdvance;

    [Header("NPC Info")]
    [Tooltip("이 NPC의 이름입니다. DialogUI에 표시됩니다.")]
    [SerializeField] private string npcName;

    [Header("Dialogue Content")]
    [Tooltip("이 NPC가 시작할 첫 번째 대화 노드(DialogueNode)입니다.")]
    [SerializeField]
    private DialogueNode startingNode;

    [Header("Tutorial Trigger")]
    [Tooltip("대화 종료 시 띄울 튜토리얼 레벨 (0 = 없음, 1 = 1레벨, 2 = 2레벨, 3 = 3레벨)")]
    [SerializeField] private int tutorialLevelToShow = 0; // [수정됨]

    private DialogueNode currentNode;
    [SerializeField]
    public DialogueState currentState;

    public Coroutine currentDialogueCoroutine { get; private set; }

    private void Awake()
    {
        currentState = DialogueState.Inactive;
    }

    /// <summary>
    /// InteractionComponent가 호출하는 인터페이스 구현
    /// </summary>
    public void Interact()
    {
        StartDialogue();
    }

    /// <summary>
    /// 대화를 시작합니다.
    /// </summary>
    public void StartDialogue()
    {
        if (UIManager.instance != null)
        {
            UIManager.instance.StartDialogue(this);
        }

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
            // 대화 종료
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
    /// [수정됨] 대화가 완전히 종료될 때 UIManager에게 상태 복구를 요청합니다.
    /// </summary>
    private IEnumerator EndInteractionCoroutine()
    {
        if (UIManager.instance != null)
        {
            // [수정] 튜토리얼 레벨 번호를 UIManager에게 전달
            UIManager.instance.EndDialogue(tutorialLevelToShow);
        }

        yield return new WaitForEndOfFrame();

        // [수정] 레벨별 튜토리얼이 뜰 경우, 상호작용 종료를 미룹니다.
        // (튜토리얼이 닫힐 때 UIManager가 대신 호출해줍니다)
        if (tutorialLevelToShow == 0)
        {
            InteractionComponent.EndInteraction();
        }
    }
}