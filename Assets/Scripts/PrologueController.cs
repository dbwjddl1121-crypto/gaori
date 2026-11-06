using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class PrologueController : MonoBehaviour
{
    [SerializeField] VideoPlayer videoPlayer;

    bool finished;

    void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        videoPlayer.errorReceived += (vp, msg) => Debug.LogError("[Prologue] 영상 에러: " + msg);
        videoPlayer.loopPointReached += _ => Finish();
    }

    public void OnClickSkip() => Finish();

    void Finish()
    {
        if (finished) return;
        finished = true;
        PlayerPrefs.SetInt(TitleStartRouter.PrologueKey, 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("GameScene");
    }
}