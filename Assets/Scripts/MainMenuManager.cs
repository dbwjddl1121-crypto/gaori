using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // 시작하기 버튼을 눌렀을 때 실행될 함수
    public void GameStart()
    {
        SceneManager.LoadScene("GameScene");
    }

    // 설정 버튼을 눌렀을 때 실행될 함수
    public void OpenSettings()
    {
        Debug.Log("설정 창 열기");
    }

    // 종료 버튼을 눌렀을 때 실행될 함수
    public void GameExit()
    {
        Debug.Log("게임 종료");
        Application.Quit();
    }
}