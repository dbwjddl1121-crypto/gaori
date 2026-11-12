using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class EndingController : MonoBehaviour
{
    // 어떤 엔딩을 재생할지 (엔딩 씬을 열기 전에 설정)
    public enum EndingType { Happy, Bad }
    public static EndingType pending = EndingType.Happy;

    [Header("연결")]
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] VideoClip happyClip;
    [SerializeField] VideoClip badClip;

    [Header("이동할 씬")]
    [SerializeField] string titleSceneName = "TitleScene";

    bool finished;

    void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        VideoClip clip = (pending == EndingType.Happy) ? happyClip : badClip;

        videoPlayer.errorReceived += (vp, msg) =>
        {
            Debug.LogError("[Ending] 영상 에러: " + msg);
            Finish();
        };
        videoPlayer.loopPointReached += _ => Finish();

        if (clip == null)
        {
            // 영상이 아직 없을 때: 바로 마무리 (임시 대응)
            Debug.LogWarning("[Ending] 영상이 연결되지 않아 바로 넘어갑니다.");
            Finish();
            return;
        }

        videoPlayer.clip = clip;
        videoPlayer.isLooping = false;
        videoPlayer.Play();
    }

    // 건너뛰기 버튼용
    public void OnClickSkip() => Finish();

    void Finish()
    {
        if (finished) return;
        finished = true;

        // 해피/배드 상관없이 전부 초기화 후 타이틀로
        GameReset.ResetAll();

        SceneManager.LoadScene(titleSceneName);
    }
}