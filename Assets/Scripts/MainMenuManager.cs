using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("설정 패널")]
    public GameObject settingsPanel;

    [Header("설정창이 열리면 숨길 메뉴 버튼")]
    public GameObject startButton;
    public GameObject settingButton;
    public GameObject exitButton;

    void Start()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        SetMenuButtons(true);
    }

    // 시작하기 버튼을 눌렀을 때 실행될 함수
    public void GameStart()
    {
        SceneManager.LoadScene("GameScene");
    }

    // 설정 버튼을 눌렀을 때 실행될 함수
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }

        SetMenuButtons(false);
    }

    // 설정 창의 닫기 버튼을 눌렀을 때 실행될 함수
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        SetMenuButtons(true);
    }

    // 종료 버튼은 현재 SceneFader.FadeToQuit()을 사용 중
    public void GameExit()
    {
        Debug.Log("게임 종료");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void SetMenuButtons(bool show)
    {
        if (startButton != null) startButton.SetActive(show);
        if (settingButton != null) settingButton.SetActive(show);
        if (exitButton != null) exitButton.SetActive(show);
    }
}