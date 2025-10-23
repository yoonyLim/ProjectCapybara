using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 화면 캔버스에 대화창을 표시하고, 타이핑 효과와 오디오를 담당합니다.
/// UIManager가 이 UI를 켜고 끕니다.
/// </summary>
public class DialogUI : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("UIManager가 켜고 끌 최상위 UI 패널")]
    public GameObject dialogBox;
    [Tooltip("대화 텍스트가 표시될 TextMeshPro UI")]
    public TextMeshProUGUI dialogText;
    [Tooltip("다음으로 넘어가기 버튼 (UIManager의 'dialogUIFirstButton'에 연결)")]
    public Button nextButton;

    [Header("Typing Effect & Audio")]
    [SerializeField] private float timeBetweenCharacters = 0.05f;
    [SerializeField] private int dialogueBeepCharacterInterval = 2;
    [SerializeField] private AudioClip[] speechBeepSounds; // 비프음

    private AudioSource dialogueAudioSource;

    // 다른 스크립트(Dialogue.cs)가 쉽게 접근할 수 있는 싱글톤
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

        // 오디오 소스 설정 (없으면 추가)
        dialogueAudioSource = GetComponent<AudioSource>();
        if (dialogueAudioSource == null)
        {
            dialogueAudioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Start()
    {
        // '다음' 버튼 클릭 시 UIManager의 '다음 단계' 함수를 호출하도록 연결
        if (nextButton != null && UIManager.instance != null)
        {
            nextButton.onClick.AddListener(() => UIManager.instance.TriggerNextDialogueStep());
        }
    }

    /// <summary>
    /// Dialogue.cs가 호출: 새 대화 텍스트를 받아 타이핑 효과를 시작합니다.
    /// </summary>
    public Coroutine ShowDialog(string message)
    {
        // dialogBox.SetActive(true); // UIManager가 AnimateOpen으로 켭니다.
        dialogText.text = message; // 텍스트 미리 설정
        return StartCoroutine(TypeText(message));
    }

    /// <summary>
    /// 텍스트에 타이핑 효과와 비프음을 적용하는 코루틴입니다.
    /// </summary>
    private IEnumerator TypeText(string message)
    {
        // 타이핑 중에는 '다음' 버튼 비활성화 (포커스는 유지됨)
        nextButton.interactable = false;

        dialogText.maxVisibleCharacters = 0;

        for (int i = 0; i < message.Length; i++)
        {
            // 비프음 재생
            if (i % dialogueBeepCharacterInterval == 0 && speechBeepSounds.Length > 0)
            {
                int randomIndex = Random.Range(0, speechBeepSounds.Length);
                dialogueAudioSource.PlayOneShot(speechBeepSounds[randomIndex]);
            }

            dialogText.maxVisibleCharacters++;

            // [수정] Time.timeScale이 0일 때도 작동하도록 WaitForSecondsRealtime을 사용합니다.
            yield return new WaitForSecondsRealtime(timeBetweenCharacters);
        }

        // 타이핑 완료 후 '다음' 버튼 다시 활성화
        nextButton.interactable = true;
    }
}