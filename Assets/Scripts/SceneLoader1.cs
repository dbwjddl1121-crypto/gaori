using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader1 : MonoBehaviour
{
    public string nextSceneName = "REBLUE_Backup";
    public float delaySeconds = 0.5f; // 코인 UI 잠깐이라도 뜨게 살짝 딜레이 (0이면 바로 전환)

    void Start()
    {
        Invoke(nameof(LoadNextScene), delaySeconds);
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
