using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("슬라이더 / 토글")]
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Toggle fullscreenToggle;

    // 다른 스크립트에서 SettingsManager.BGMVolume 처럼 읽어서 쓸 수 있어요
    public static float BGMVolume = 0.7f;
    public static float SFXVolume = 0.7f;

    const string BGM_KEY = "BGMVolume";
    const string SFX_KEY = "SFXVolume";
    const string FULLSCREEN_KEY = "Fullscreen";

    void Start()
    {
        // 저장된 값 불러오기 (없으면 기본값)
        BGMVolume = PlayerPrefs.GetFloat(BGM_KEY, 0.7f);
        SFXVolume = PlayerPrefs.GetFloat(SFX_KEY, 0.7f);
        bool isFullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) == 1;

        // UI에 반영 (이벤트는 발생시키지 않음)
        if (bgmSlider != null)
        {
            bgmSlider.SetValueWithoutNotify(BGMVolume);
            bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(SFXVolume);
            sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.SetIsOnWithoutNotify(isFullscreen);
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }

        // 저장된 화면 모드 적용
        Screen.fullScreen = isFullscreen;
    }

    void OnBGMChanged(float value)
    {
        BGMVolume = value;
        PlayerPrefs.SetFloat(BGM_KEY, value);
        PlayerPrefs.Save();
        Debug.Log("BGM 볼륨: " + value);
    }

    void OnSFXChanged(float value)
    {
        SFXVolume = value;
        PlayerPrefs.SetFloat(SFX_KEY, value);
        PlayerPrefs.Save();
        Debug.Log("효과음 볼륨: " + value);
    }

    void OnFullscreenChanged(bool isOn)
    {
        Screen.fullScreen = isOn;
        PlayerPrefs.SetInt(FULLSCREEN_KEY, isOn ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log("전체화면: " + isOn);
    }
}