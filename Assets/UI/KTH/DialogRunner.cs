using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Stack을 사용하기 위해 필요

public class DialogueRunner : MonoBehaviour
{
    [SerializeField] private DialogueTree dialogueTree;
    private DialogueNode currentNode;

    // 거쳐온 노드들을 기억하기 위한 스택(Stack)
    private Stack<DialogueNode> history = new Stack<DialogueNode>();

    private Coroutine dialogueCoroutine;
    private bool isTyping = false;

    public bool IsDialogueActive { get; private set; }

    private void Start()
    {
        // UI 버튼에 '이전', '다음' 기능을 연결
        if (DialogUI.Instance != null)
        {
            DialogUI.Instance.SetupNavigationButtons(GoToPreviousNode, GoToNextNode);
        }
    }

    public void StartDialogue()
    {
        if (dialogueTree == null)
        {
            Debug.LogError("DialogueTree가 할당되지 않았습니다!");
            return;
        }

        IsDialogueActive = true;
        history.Clear(); // 새 대화 시작 시 이력 초기화
        currentNode = dialogueTree.RootNode;
        ProcessCurrentNode();
    }

    // '다음' 버튼을 눌렀을 때 호출될 함수
    public void GoToNextNode()
    {
        // 타이핑 중이면 무시
        if (isTyping) return;

        // 다음 노드가 있으면 진행
        if (currentNode != null && currentNode.NextNode != null)
        {
            history.Push(currentNode); // 현재 노드를 이력에 추가
            currentNode = currentNode.NextNode;
            ProcessCurrentNode();
        }
        else
        {
            // 다음 노드가 없으면 대화 종료
            EndDialogue();
        }
    }

    // '이전' 버튼을 눌렀을 때 호출될 함수
    public void GoToPreviousNode()
    {
        // 타이핑 중이거나, 이력이 없으면 무시
        if (isTyping || history.Count == 0) return;

        // 이력에서 이전 노드를 꺼내와 현재 노드로 설정
        currentNode = history.Pop();
        ProcessCurrentNode();
    }

    private void ProcessCurrentNode()
    {
        if (dialogueCoroutine != null)
        {
            StopCoroutine(dialogueCoroutine);
        }
        dialogueCoroutine = StartCoroutine(ProcessNodeCoroutine());
    }

    private IEnumerator ProcessNodeCoroutine()
    {
        isTyping = true;
        yield return DialogUI.Instance.ShowDialog(currentNode.DialogueText);
        isTyping = false;

        // 선택지 기반 로직은 이제 사용하지 않으므로 주석 처리 또는 삭제
        // if (currentNode.Choices.Count > 0)
        // {
        //     DialogUI.Instance.ShowChoices(currentNode.Choices, OnChoiceSelected);
        // }
    }

    private void EndDialogue()
    {
        IsDialogueActive = false;
        DialogUI.Instance.CloseDialog();
        Debug.Log("대화가 종료되었습니다.");
    }

    // 테스트용 Update 함수
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && !IsDialogueActive)
        {
            StartDialogue();
        }
    }
}