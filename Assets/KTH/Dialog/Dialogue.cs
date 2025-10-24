using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// [���� ����] NPC�� �����Ǿ� ��ȭ�� '�帧' (� �������)�� �����մϴ�.
/// ���� ǥ��(UI, �����)�� DialogUI.cs�� �����մϴ�.
/// </summary>
public class Dialogue : MonoBehaviour, IInteractable
{
    public enum DialogueState
    {
        Inactive,
        Normal
    }

    private bool blockInteraction = false;

    public event Action OnDialogueStart;
    public event Action OnDialogueEnd;
    public event Action OnDialogueAdvance;

    [Header("NPC Info")]
    [Tooltip("�� NPC�� �̸��Դϴ�. DialogUI�� ǥ�õ˴ϴ�.")]
    [SerializeField] private string npcName;

    [Header("Dialogue Content")]
    [Tooltip("�� NPC�� ������ ù ��° ��ȭ ���(DialogueNode)�Դϴ�.")]
    [SerializeField]
    private DialogueNode startingNode;

    [Header("Tutorial Trigger")]
    [Tooltip("��ȭ ���� �� ��� Ʃ�丮�� ���� (0 = ����, 1 = 1����, 2 = 2����, 3 = 3����)")]
    [SerializeField] private int tutorialLevelToShow = 0; // [������]

    private DialogueNode currentNode;
    [SerializeField]
    public DialogueState currentState;

    public Coroutine currentDialogueCoroutine { get; private set; }

    private void Awake()
    {
        currentState = DialogueState.Inactive;
    }

    /// <summary>
    /// InteractionComponent�� ȣ���ϴ� �������̽� ����
    /// </summary>
    public void Interact()
    {
        if (blockInteraction) return;
        StartDialogue();
    }

    /// <summary>
    /// ��ȭ�� �����մϴ�.
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
            Debug.LogWarning("DialogUI �ν��Ͻ��� ã�� �� ���� �̸��� ������ �� �����ϴ�.");
        }

        OnDialogueStart?.Invoke();

        if (startingNode == null)
        {
            Debug.LogError($"{gameObject.name}�� startingNode�� �Ҵ���� �ʾҽ��ϴ�!");
            StartCoroutine(EndInteractionCoroutine());
            return;
        }

        currentNode = startingNode;
        currentState = DialogueState.Normal;

        currentDialogueCoroutine = StartCoroutine(ProcessDialogue());
    }

    /// <summary>
    /// ���� ��带 ó���ϰ�, DialogUI�� �ؽ�Ʈ ǥ�ø� '��û'�մϴ�.
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
                Debug.LogError("DialogUI �ν��Ͻ��� ã�� �� �����ϴ�!");
                yield return null;
            }

            currentNode = currentNode.NextNode;
        }
        else
        {
            // ��ȭ ����
            currentState = DialogueState.Inactive;
            StartCoroutine(EndInteractionCoroutine());
            OnDialogueEnd?.Invoke();
        }

        currentDialogueCoroutine = null;
    }

    /// <summary>
    /// UIManager�� '����' �Է��� �޾��� �� ȣ���� ���� �Լ��Դϴ�.
    /// </summary>
    public void AdvanceDialogue()
    {
        if (currentState == DialogueState.Normal && currentDialogueCoroutine == null)
        {
            currentDialogueCoroutine = StartCoroutine(ProcessDialogue());
        }
    }

    /// <summary>
    /// [������] ��ȭ�� ������ ����� �� UIManager���� ���� ������ ��û�մϴ�.
    /// </summary>
    private IEnumerator EndInteractionCoroutine()
    {
        if (UIManager.instance != null)
        {
            // [����] Ʃ�丮�� ���� ��ȣ�� UIManager���� ����
            UIManager.instance.EndDialogue(tutorialLevelToShow);
        }

        yield return new WaitForEndOfFrame();

        // [����] ������ Ʃ�丮���� �� ���, ��ȣ�ۿ� ���Ḧ �̷�ϴ�.
        // (Ʃ�丮���� ���� �� UIManager�� ��� ȣ�����ݴϴ�)
        if (tutorialLevelToShow == 0)
        {
            InteractionComponent.EndInteraction();
        }
        
        StartCoroutine(BlockInteractionCoroutine(0.5f));
    }

    IEnumerator BlockInteractionCoroutine(float duration)
    {
        blockInteraction = true;
        
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        blockInteraction = false;
    }
}