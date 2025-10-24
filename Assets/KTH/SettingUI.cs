using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class SettingsUI : MonoBehaviour
{
    [Header("DISPLAY")]
    [SerializeField] private TextMeshProUGUI resolutionText;
    [SerializeField] private Button prevResolutionButton;
    [SerializeField] private Button nextResolutionButton;
    [SerializeField] private TextMeshProUGUI screenModeText;
    [SerializeField] private Button prevScreenModeButton;
    [SerializeField] private Button nextScreenModeButton;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private PostProcessVolume postProcessVolume;

    [Header("AUDIO")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("AUDIO ICONS")]
    [SerializeField] private Image masterVolumeIcon;
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;
    [SerializeField] private Image bgmVolumeIcon;
    [SerializeField] private Sprite bgmSoundOnSprite;
    [SerializeField] private Sprite bgmSoundOffSprite;
    [SerializeField] private Image sfxVolumeIcon;
    [SerializeField] private Sprite sfxSoundOnSprite;
    [SerializeField] private Sprite sfxSoundOffSprite;

    private ColorGrading colorGrading;

    void Start()
    {
        if (postProcessVolume != null)
        {
            postProcessVolume.profile = Instantiate(postProcessVolume.profile);
            if (!postProcessVolume.profile.TryGetSettings(out colorGrading))
            {
                Debug.LogError("Color Grading ������ ã�� ���߽��ϴ�!");
            }
        }
    }

    void OnEnable()
    {
        LoadSettingsToUI();
        AddListeners();
    }

    void OnDisable()
    {
        RemoveListeners();
    }

    private void AddListeners()
    {
        prevResolutionButton.onClick.AddListener(OnPrevResolutionClicked);
        nextResolutionButton.onClick.AddListener(OnNextResolutionClicked);
        prevScreenModeButton.onClick.AddListener(OnScreenModeButtonClicked);
        nextScreenModeButton.onClick.AddListener(OnScreenModeButtonClicked);
        brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        bgmVolumeSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
    }

    private void RemoveListeners()
    {
        prevResolutionButton.onClick.RemoveAllListeners();
        nextResolutionButton.onClick.RemoveAllListeners();
        prevScreenModeButton.onClick.RemoveAllListeners();
        nextScreenModeButton.onClick.RemoveAllListeners();
        brightnessSlider.onValueChanged.RemoveAllListeners();
        masterVolumeSlider.onValueChanged.RemoveAllListeners();
        bgmVolumeSlider.onValueChanged.RemoveAllListeners();
        sfxVolumeSlider.onValueChanged.RemoveAllListeners();
    }

    private void LoadSettingsToUI()
    {
        if (SettingsManager.instance == null) return;

        masterVolumeSlider.value = SettingsManager.instance.MasterVolume;
        UpdateMasterVolumeIcon(masterVolumeSlider.value);

        bgmVolumeSlider.value = SettingsManager.instance.BgmVolume;
        UpdateBgmVolumeIcon(bgmVolumeSlider.value);

        sfxVolumeSlider.value = SettingsManager.instance.SfxVolume;
        UpdateSfxVolumeIcon(sfxVolumeSlider.value);

        brightnessSlider.value = SettingsManager.instance.Brightness;

        UpdateResolutionUI();
        UpdateScreenModeUI();
        UpdateBrightness();
    }

    private void UpdateResolutionUI()
    {
        if (SettingsManager.instance == null) return;
        Resolution currentRes = SettingsManager.instance.Resolutions[SettingsManager.instance.ResolutionIndex];
        resolutionText.text = $"{currentRes.width}x{currentRes.height}";
    }

    private void UpdateScreenModeUI()
    {
        if (SettingsManager.instance == null) return;
        if (SettingsManager.instance.IsFullscreen)
        {
            screenModeText.text = "Fullscreen";
        }
        else
        {
            screenModeText.text = "Windowed";
        }
    }

    private void UpdateBrightness()
    {
        if (colorGrading != null)
        {
            float exposureValue = (SettingsManager.instance.Brightness - 0.5f) * 3f;
            colorGrading.postExposure.value = exposureValue;
        }
    }

    public void OnPrevResolutionClicked()
    {
        SettingsManager.instance.CycleResolution(false);
        UpdateResolutionUI();
    }

    public void OnNextResolutionClicked()
    {
        SettingsManager.instance.CycleResolution(true);
        UpdateResolutionUI();
    }

    public void OnScreenModeButtonClicked()
    {
        SettingsManager.instance.ToggleFullscreen();
        UpdateScreenModeUI();
    }

    public void OnBrightnessChanged(float value)
    {
        SettingsManager.instance.SetBrightness(value);
        UpdateBrightness();
    }

    public void OnMasterVolumeChanged(float value)
    {
        SettingsManager.instance.SetMasterVolume(value);
        UpdateMasterVolumeIcon(value);
    }

    public void OnBgmVolumeChanged(float value)
    {
        SettingsManager.instance.SetBgmVolume(value);
        UpdateBgmVolumeIcon(value);
    }

    public void OnSfxVolumeChanged(float value)
    {
        SettingsManager.instance.SetSfxVolume(value);
        UpdateSfxVolumeIcon(value);
    }

    private void UpdateMasterVolumeIcon(float value)
    {
        if (masterVolumeIcon == null || soundOnSprite == null || soundOffSprite == null) return;
        if (value <= 0.01f)
        {
            masterVolumeIcon.sprite = soundOffSprite;
        }
        else
        {
            masterVolumeIcon.sprite = soundOnSprite;
        }
    }

    private void UpdateBgmVolumeIcon(float value)
    {
        if (bgmVolumeIcon == null || bgmSoundOnSprite == null || bgmSoundOffSprite == null) return;
        if (value <= 0.01f)
        {
            bgmVolumeIcon.sprite = bgmSoundOffSprite;
        }
        else
        {
            bgmVolumeIcon.sprite = bgmSoundOnSprite;
        }
    }

    private void UpdateSfxVolumeIcon(float value)
    {
        if (sfxVolumeIcon == null || sfxSoundOnSprite == null || sfxSoundOffSprite == null) return;
        if (value <= 0.01f)
        {
            sfxVolumeIcon.sprite = sfxSoundOffSprite;
        }
        else
        {
            sfxVolumeIcon.sprite = sfxSoundOnSprite;
        }
    }

    public void SaveAllSettings()
    {
        if (SettingsManager.instance != null)
        {
            SettingsManager.instance.SaveSettings();
        }
    }
}