using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using System.Collections.Generic;

public class SettingsUI : MonoBehaviour
{
    [Header("DISPLAY")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private PostProcessVolume postProcessVolume;

    [Header("GRAPHIC")]
    [SerializeField] private Toggle pixelFilterToggle;

    [Header("AUDIO")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    private ColorGrading colorGrading;

    void Start()
    {
        if (postProcessVolume != null)
        {
            // 런타임용 프로파일 복제본 생성 (원본 에셋을 보호하고 더 안정적)
            postProcessVolume.profile = Instantiate(postProcessVolume.profile);

            // 프로파일에서 ColorGrading 설정을 찾는지 확인
            if (postProcessVolume.profile.TryGetSettings(out colorGrading))
            {
                Debug.Log("<color=green>성공:</color> Color Grading 설정을 찾았습니다.");
            }
            else
            {
                Debug.LogError("<color=red>실패:</color> Post Process Volume 프로파일에서 Color Grading 설정을 찾지 못했습니다! 인스펙터에서 Profile에 Color Grading 효과를 추가했는지 확인해주세요.");
            }
        }
        else
        {
            Debug.LogError("<color=red>실패:</color> PostProcessVolume이 SettingsUI 스크립트에 연결되지 않았습니다! 인스펙터 창을 확인해주세요.");
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
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        pixelFilterToggle.onValueChanged.AddListener(OnPixelFilterChanged);
        brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        bgmVolumeSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
    }

    private void RemoveListeners()
    {
        resolutionDropdown.onValueChanged.RemoveAllListeners();
        fullscreenToggle.onValueChanged.RemoveAllListeners();
        pixelFilterToggle.onValueChanged.RemoveAllListeners();
        brightnessSlider.onValueChanged.RemoveAllListeners();
        masterVolumeSlider.onValueChanged.RemoveAllListeners();
        bgmVolumeSlider.onValueChanged.RemoveAllListeners();
        sfxVolumeSlider.onValueChanged.RemoveAllListeners();
    }

    private void LoadSettingsToUI()
    {
        if (SettingsManager.instance == null) return;

        masterVolumeSlider.value = SettingsManager.instance.MasterVolume;
        bgmVolumeSlider.value = SettingsManager.instance.BgmVolume;
        sfxVolumeSlider.value = SettingsManager.instance.SfxVolume;
        fullscreenToggle.isOn = SettingsManager.instance.IsFullscreen;
        pixelFilterToggle.isOn = SettingsManager.instance.PixelFilter;
        brightnessSlider.value = SettingsManager.instance.Brightness;

        SetupResolutionDropdown();
        UpdateBrightness();
    }

    private void SetupResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        Resolution[] resolutions = SettingsManager.instance.Resolutions;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = SettingsManager.instance.ResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void UpdateBrightness()
    {
        if (colorGrading != null)
        {
            float exposureValue = (SettingsManager.instance.Brightness - 0.5f) * 3f;
            colorGrading.postExposure.value = exposureValue;

            // 슬라이더를 움직일 때마다 현재 적용하려는 값을 콘솔에 출력
            Debug.Log("밝기 값 적용 시도: " + exposureValue);
        }
        else
        {
            Debug.LogWarning("밝기 조절 실패: ColorGrading 설정이 없습니다.");
        }
    }

    #region UI Event Handlers
    private void OnResolutionChanged(int index) => SettingsManager.instance.SetResolution(index);
    private void OnFullscreenChanged(bool isFull) => SettingsManager.instance.SetFullscreen(isFull);
    private void OnPixelFilterChanged(bool isActive) => SettingsManager.instance.SetPixelFilter(isActive);
    private void OnBrightnessChanged(float value)
    {
        SettingsManager.instance.SetBrightness(value);
        UpdateBrightness();
    }
    private void OnMasterVolumeChanged(float value) => SettingsManager.instance.SetMasterVolume(value);
    private void OnBgmVolumeChanged(float value) => SettingsManager.instance.SetBgmVolume(value);
    private void OnSfxVolumeChanged(float value) => SettingsManager.instance.SetSfxVolume(value);
    #endregion

    public void SaveAllSettings()
    {
        if (SettingsManager.instance != null)
        {
            SettingsManager.instance.SaveSettings();
        }
    }
}