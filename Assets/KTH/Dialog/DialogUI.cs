using UnityEngine;
using UnityEngine.UI;
using TMPro; // [추가됨] TextMeshPro 사용을 위해
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

    [Tooltip("NPC 이름이 표시될 TextMeshPro UI")]
    [SerializeField] private TextMeshProUGUI nameText; // [추가됨]

    [Tooltip("대화 텍스트가 표시될 TextMeshPro UI")]
    public TextMeshProUGUI dialogText;

    [Tooltip("다음으로 넘어가기 버튼 (UIManager의 'dialogUIFirstButton'에 연결)")]
    public Button nextButton;

    [Header("Typing Effect & Audio")]
    [SerializeField] private float timeBetweenCharacters = 0.05f;
    [SerializeField] private int dialogueBeepCharacterInterval = 2;
    [SerializeField] private AudioClip[] speechBeepSounds;

    [Header("Name Font Settings")] // [추가됨]
    [Tooltip("이름 텍스트에 적용할 폰트 에셋 (TMP Font Asset)")]
    [SerializeField] private TMP_FontAsset nameFont;
    [Tooltip("이름 텍스트의 폰트 크기")]
    [SerializeField] private float nameFontSize = 42f;

    [Header("Dialogue Font Settings")] // [추가됨]
    [Tooltip("대화 텍스트에 적용할 폰트 에셋 (TMP Font Asset)")]
    [SerializeField] private TMP_FontAsset dialogueFont;
    [Tooltip("대화 텍스트의 폰트 크기")]
    [SerializeField] private float dialogueFontSize = 36f;

    private AudioSource dialogueAudioSource;
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

        dialogueAudioSource = GetComponent<AudioSource>();
        if (dialogueAudioSource == null)
        {
            dialogueAudioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Start()
    {
        // '다음' 버튼 리스너 연결
        if (nextButton != null && UIManager.instance != null)
        {
            nextButton.onClick.AddListener(() => UIManager.instance.TriggerNextDialogueStep());
        }

        // [추가됨] 폰트 설정 적용
        ApplyFontSettings();
    }

    /// <summary>
    /// [추가됨] 인스펙터에서 설정한 폰트와 크기를 UI 텍스트에 적용합니다.
    /// </summary>
    private void ApplyFontSettings()
    {
        // 이름 텍스트 폰트 적용
        if (nameText != null)
        {
            if (nameFont != null)
                nameText.font = nameFont;
            if (nameFontSize > 0)
                nameText.fontSize = nameFontSize;
        }

        // 대화 텍스트 폰트 적용
        if (dialogText != null)
        {
            if (dialogueFont != null)
                dialogText.font = dialogueFont;
            if (dialogueFontSize > 0)
                dialogText.fontSize = dialogueFontSize;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// [추가됨] 유니티 에디터에서 값을 변경할 때마다 즉시 미리보기
    /// </summary>
    private void OnValidate()
    {
        ApplyFontSettings();
    }
#endif

    /// <summary>
    /// [추가됨] Dialogue.cs가 호출: NPC 이름을 설정합니다.
    /// </summary>
    public void SetNpcName(string name)
    {
        if (nameText != null)
        {
            if (string.IsNullOrEmpty(name))
            {
                // 이름이 비어있으면 이름 UI를 숨깁니다.
                nameText.gameObject.SetActive(false);
            }
            else
            {
                nameText.gameObject.SetActive(true);
                nameText.text = name;
            }
        }
    }

    /// <summary>
    /// Dialogue.cs가 호출: 새 대화 텍스트를 받아 타이핑 효과를 시작합니다.
    /// </summary>
    public Coroutine ShowDialog(string message)
    {
        dialogText.text = message; // 텍스트 미리 설정
        return StartCoroutine(TypeText(message));
    }

    /// <summary>
    /// 텍스트에 타이핑 효과와 비프음을 적용하는 코루틴입니다.
    /// </summary>
    private IEnumerator TypeText(string message)
    {
        nextButton.interactable = false;
        dialogText.maxVisibleCharacters = 0;

        for (int i = 0; i < message.Length; i++)
        {
            if (i % dialogueBeepCharacterInterval == 0 && speechBeepSounds.Length > 0)
            {
                int randomIndex = Random.Range(0, speechBeepSounds.Length);
                dialogueAudioSource.PlayOneShot(speechBeepSounds[randomIndex]);
            }

            dialogText.maxVisibleCharacters++;
            yield return new WaitForSecondsRealtime(timeBetweenCharacters);
        }

        nextButton.interactable = true;
    }
}