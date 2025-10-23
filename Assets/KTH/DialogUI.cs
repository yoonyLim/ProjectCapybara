using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class DialogUI : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject dialogBox;
    public TextMeshProUGUI dialogText;
    public Button previousButton; // 이전 버튼
    public Button nextButton;     // 다음 버튼

    [Header("Typing Effect")]
    [SerializeField] private float timeBetweenCharacters = 0.05f;

    public static DialogUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CloseDialog()
    {
        dialogBox.SetActive(false);
    }

    public Coroutine ShowDialog(string message)
    {
        dialogBox.SetActive(true);
        dialogText.text = message;
        return StartCoroutine(TypeText(message));
    }

    private IEnumerator TypeText(string message)
    {
        previousButton.interactable = false; // 타이핑 중에는 버튼 비활성화
        nextButton.interactable = false;

        dialogText.maxVisibleCharacters = 0;
        for (int i = 0; i < message.Length; i++)
        {
            dialogText.maxVisibleCharacters++;
            yield return new WaitForSeconds(timeBetweenCharacters);
        }

        previousButton.interactable = true; // 타이핑 끝나면 버튼 활성화
        nextButton.interactable = true;
    }

    // DialogueRunner가 버튼에 기능을 연결할 수 있도록 하는 함수
    public void SetupNavigationButtons(Action onPrevious, Action onNext)
    {
        previousButton.onClick.RemoveAllListeners();
        previousButton.onClick.AddListener(() => onPrevious?.Invoke());

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() => onNext?.Invoke());
    }
}