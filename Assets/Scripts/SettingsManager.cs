using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("슬라이더 / 토글")]
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Toggle fullscreenToggle;

    [Header("창 모드 처음 크기")]
    public int windowedWidth = 1280;
    public int windowedHeight = 720;

    // 다른 스크립트에서 SettingsManager.BGMVolume 처럼 읽어서 쓸 수 있어요
    public static float BGMVolume = 0.7f;
    public static float SFXVolume = 0.7f;

    const string BGM_KEY = "BGMVolume";
    const string SFX_KEY = "SFXVolume";
    const string FULLSCREEN_KEY = "Fullscreen";

    // 게임을 켠 뒤 화면 모드를 이미 적용했는지 (씬이 바뀌어도 유지됨)
    static bool screenApplied = false;

    // 마지막으로 쓴 창 모드 크기 (0이면 아직 없음)
    static int lastWindowW = 0;
    static int lastWindowH = 0;

    void Start()
    {
        // 저장된 값 불러오기 (없으면 기본값). 전체화면은 처음엔 창 모드(0)로 시작
        BGMVolume = PlayerPrefs.GetFloat(BGM_KEY, 0.7f);
        SFXVolume = PlayerPrefs.GetFloat(SFX_KEY, 0.7f);
        bool isFullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, 0) == 1;

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

        // 화면 모드는 게임을 켠 뒤 처음 한 번만 적용 (씬이 바뀔 때마다 창 크기가 초기화되는 걸 막음)
        if (!screenApplied)
        {
            screenApplied = true;
            ApplyScreenMode(isFullscreen);
        }
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
        // 창 모드에서 전체화면으로 가기 전에, 지금 창 크기를 기억해 둠
        if (isOn && !Screen.fullScreen)
        {
            lastWindowW = Screen.width;
            lastWindowH = Screen.height;
        }

        ApplyScreenMode(isOn);
        PlayerPrefs.SetInt(FULLSCREEN_KEY, isOn ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log("전체화면: " + isOn);
    }

    // 체크하면 전체화면, 끄면 창 모드로 바꿈
    void ApplyScreenMode(bool fullscreen)
    {
        if (fullscreen)
        {
            // 모니터 해상도에 맞춰 전체화면
            Screen.SetResolution(
                Display.main.systemWidth,
                Display.main.systemHeight,
                FullScreenMode.FullScreenWindow);
        }
        else
        {
            // 기억해 둔 창 크기가 있으면 그 크기로, 없으면 처음 크기로
            int w = lastWindowW > 0 ? lastWindowW : windowedWidth;
            int h = lastWindowH > 0 ? lastWindowH : windowedHeight;

            Screen.SetResolution(w, h, FullScreenMode.Windowed);
        }
    }
}