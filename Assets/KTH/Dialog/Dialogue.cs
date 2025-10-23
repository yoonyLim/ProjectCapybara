using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// [로직 전용] NPC에 부착되어 대화의 '흐름' (어떤 노드인지)만 관리합니다.
/// 실제 표현(UI, 오디오)은 DialogUI.cs에 위임합니다.
/// </summary>
//[RequireComponent(typeof(Animal))]
public class Dialogue : MonoBehaviour
{
    // 오디오, 타이핑, 말풍선(speechBubbleText) 관련 필드 모두 제거

    public enum DialogueState
    {
        Inactive,
        Normal
    }

    public event Action OnDialogueStart;
    public event Action OnDialogueEnd;
    public event Action OnDialogueAdvance;

    [Header("Dialogue Content")]
    [Tooltip("이 NPC가 시작할 첫 번째 대화 노드(DialogueNode)입니다.")]
    [SerializeField]
    private DialogueNode startingNode;

    private DialogueNode currentNode; // 현재 진행 중인 대화 노드

    [SerializeField]
    public DialogueState currentState;

    // Animal 스크립트로 전달될 표정 (이건 남겨둡니다)
    //public Animal.FacialAnimationType TargetFacialAnimation;

    // 회전, 오디오, 타이핑 코루틴 필드 모두 제거
    public Coroutine currentDialogueCoroutine { get; private set; } // 타이핑 코루틴 (DialogUI의 코루틴을 참조)

    private void Awake()
    {
        // speechBubbleText, originalRotation 관련 코드 모두 제거
        currentState = DialogueState.Inactive;
    }

    /// <summary>
    /// 대화를 시작합니다. (InteractionComponent 또는 DialogueTester에서 호출)
    /// </summary>
    public void StartDialogue()
    {
        // 1. UIManager에게 대화 시작을 알립니다. (UIManager가 UI를 켭니다)
        if (UIManager.instance != null)
        {
            UIManager.instance.StartDialogue(this);
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

        // 2. ProcessDialogue를 호출하여 첫 번째 대사를 DialogUI에 표시하도록 '요청'합니다.
        // (FacePlayer 등 시각적 요소 제거)
        currentDialogueCoroutine = StartCoroutine(ProcessDialogue());
    }

    // FacePlayer, FaceOriginalRotation, RotateMesh 메서드 모두 제거

    /// <summary>
    /// 현재 노드를 처리하고, DialogUI에 텍스트 표시를 '요청'합니다.
    /// </summary>
    public IEnumerator ProcessDialogue()
    {
        if (currentNode)
        {
            // 노드 처리 (애니메이션, 텍스트)
            //TargetFacialAnimation = currentNode.FacialAnimation;
            OnDialogueAdvance?.Invoke();
            currentState = DialogueState.Normal;

            // [중요] DialogUI.Instance에 텍스트 표시 및 타이핑을 위임합니다.
            if (DialogUI.Instance != null)
            {
                yield return DialogUI.Instance.ShowDialog(currentNode.DialogueText);
            }
            else
            {
                Debug.LogError("DialogUI 인스턴스를 찾을 수 없습니다!");
                yield return null;
            }

            // 다음 노드로 이동
            currentNode = currentNode.NextNode;
        }
        else
        {
            // 대화 종료
            currentState = DialogueState.Inactive;
            StartCoroutine(EndInteractionCoroutine()); // UIManager에 종료 알림
            OnDialogueEnd?.Invoke();
        }

        currentDialogueCoroutine = null; // 타이핑 코루틴이 끝났음을 표시
    }

    // TypeText 코루틴 제거 (DialogUI로 이동)

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
        // (FaceOriginalRotation 제거)

        // 3. UIManager에게 대화 종료를 알립니다. (UIManager가 UI를 끕니다)
        if (UIManager.instance != null)
        {
            UIManager.instance.EndDialogue();
        }

        yield return new WaitForEndOfFrame();
        InteractionComponent.EndInteraction();
    }
}