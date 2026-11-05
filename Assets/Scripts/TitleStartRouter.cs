using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleStartRouter : MonoBehaviour
{
    public const string PrologueKey = "PrologueSeen";

    [SerializeField] SceneFader sceneFader;
    [Tooltip("테스트용: 체크 해제하면 페이드 없이 바로 씬 이동")]
    [SerializeField] bool useFader = true;

    public void OnClickStart()
    {
        bool seen = PlayerPrefs.GetInt(PrologueKey, 0) == 1;
        string target = seen ? "GameScene" : "PrologueScene";

        if (useFader && sceneFader != null)
            sceneFader.FadeToScene(target);
        else
            SceneManager.LoadScene(target);
    }
}