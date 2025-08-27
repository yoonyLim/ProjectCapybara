using UnityEngine;
using System.Linq;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    [Header("Audio")]
    private float masterVolume = 0.5f;
    private float bgmVolume = 0.5f;
    private float sfxVolume = 0.5f;

    [Header("Graphics")]
    private int resolutionIndex;
    private bool isFullscreen = true;
    private float brightness = 0.5f;
    private bool pixelFilter = true;
    private Resolution[] resolutions;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        resolutions = Screen.resolutions.Select(res => new Resolution { width = res.width, height = res.height }).Distinct().ToArray();
        LoadSettings();
    }

    #region Public Getters
    public float MasterVolume => masterVolume;
    public float BgmVolume => bgmVolume;
    public float SfxVolume => sfxVolume;
    public int ResolutionIndex => resolutionIndex;
    public Resolution[] Resolutions => resolutions;
    public bool IsFullscreen => isFullscreen;
    public float Brightness => brightness;
    public bool PixelFilter => pixelFilter;
    #endregion

    #region Public Setters
    public void SetMasterVolume(float volume)
    {
        masterVolume = volume;
        AudioListener.volume = masterVolume;
    }

    public void SetBgmVolume(float volume)
    {
        bgmVolume = volume;
        if (SoundManager.instance != null) SoundManager.instance.SetBGMVolume(bgmVolume);
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = volume;
        if (SoundManager.instance != null) SoundManager.instance.SetSFXVolume(sfxVolume);
    }

    public void SetResolution(int index)
    {
        if (index < 0 || index >= resolutions.Length) return;
        resolutionIndex = index;
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, isFullscreen);
        Debug.Log("요청된 해상도: " + res.width + " x " + res.height +
              " / 현재 게임 창 크기: " + Screen.width + " x " + Screen.height);
    }



    public void SetFullscreen(bool isFull)
    {
        isFullscreen = isFull;
        Screen.fullScreen = isFullscreen;
    }

    public void SetBrightness(float value)
    {
        brightness = value;
    }

    public void SetPixelFilter(bool isActive)
    {
        pixelFilter = isActive;
        Debug.Log("Pixel Filter state changed to: " + pixelFilter);
    }
    #endregion

    #region Save & Load
    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("BgmVolume", bgmVolume);
        PlayerPrefs.SetFloat("SfxVolume", sfxVolume);
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        PlayerPrefs.SetInt("IsFullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.SetFloat("Brightness", brightness);
        PlayerPrefs.SetInt("PixelFilter", pixelFilter ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        bgmVolume = PlayerPrefs.GetFloat("BgmVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SfxVolume", 1f);
        resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", resolutions.Length - 1);
        isFullscreen = PlayerPrefs.GetInt("IsFullscreen", 1) == 1;
        brightness = PlayerPrefs.GetFloat("Brightness", 1f);
        pixelFilter = PlayerPrefs.GetInt("PixelFilter", 1) == 1;

        SetMasterVolume(masterVolume);
        SetBgmVolume(bgmVolume);
        SetSfxVolume(sfxVolume);
        SetFullscreen(isFullscreen);
        SetResolution(resolutionIndex);
        SetBrightness(brightness);
        SetPixelFilter(pixelFilter);
    }
    #endregion
}