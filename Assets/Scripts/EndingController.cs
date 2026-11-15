using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class EndingController : MonoBehaviour
{
    // 어떤 엔딩을 재생할지 (엔딩 씬을 열기 전에 설정)
    public enum EndingType { Happy, Bad }
    public static EndingType pending = EndingType.Happy;

    [Header("연결")]
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] AudioSource audioSource;
    [SerializeField] VideoClip happyClip;
    [SerializeField] VideoClip badClip;
    [SerializeField] TMP_Text endingText;
    [SerializeField] Image fadeImage;

    [Header("엔딩 문구")]
    [SerializeField] string happyMessage = "HAPPY ENDING";
    [SerializeField] string badMessage = "BAD ENDING";

    [Header("연출 시간")]
    [SerializeField] float fadeDuration = 3f;
    [Tooltip("페이드가 시작된 뒤 글씨가 나타나기 시작할 때까지 기다리는 시간")]
    [SerializeField] float textDelay = 2f;
    [SerializeField] float textFadeDuration = 3f;
    [SerializeField] float textHoldDuration = 4f;

    [Header("이동할 씬")]
    [SerializeField] string titleSceneName = "TitleScene";

    bool videoEnded;
    bool finished;

    void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        bool happy = (pending == EndingType.Happy);

        // 해피 = 흰색 페이드 + 검은 글씨 / 배드 = 검은색 페이드 + 흰 글씨
        Color fadeColor = happy ? Color.white : Color.black;
        Color textColor = happy ? Color.black : Color.white;

        if (fadeImage != null)
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);

        if (endingText != null)
        {
            endingText.color = textColor;
            endingText.alpha = 0f;
        }

        VideoClip clip = happy ? happyClip : badClip;

        if (clip == null)
        {
            Debug.LogWarning("[Ending] 영상이 연결되지 않아 바로 넘어갑니다.");
            Finish();
            return;
        }

        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.clip = clip;

        // 해피는 소리 없음, 배드는 소리 있음
        if (happy || audioSource == null)
        {
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        }
        else
        {
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetTargetAudioSource(0, audioSource);
        }

        videoPlayer.errorReceived += (vp, msg) =>
        {
            Debug.LogError("[Ending] 영상 에러: " + msg);
            StartCoroutine(ShowTextAndFinish(happy, fadeColor));
        };
        videoPlayer.loopPointReached += _ =>
        {
            if (videoEnded) return;
            videoEnded = true;
            StartCoroutine(ShowTextAndFinish(happy, fadeColor));
        };

        videoPlayer.Play();
    }

    // 건너뛰기 버튼용: 텍스트도 건너뛰고 바로 타이틀로
    public void OnClickSkip() => Finish();

    IEnumerator ShowTextAndFinish(bool happy, Color fadeColor)
    {
        if (finished) yield break;

        if (endingText != null)
            endingText.text = happy ? happyMessage : badMessage;

        // 색으로 페이드아웃, 글씨는 textDelay 뒤에 천천히 등장
        float total = Mathf.Max(fadeDuration, textDelay + textFadeDuration);
        float t = 0f;
        while (t < total)
        {
            t += Time.unscaledDeltaTime;

            if (fadeImage != null)
            {
                float a = Mathf.Clamp01(t / fadeDuration);
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, a);
            }

            if (endingText != null)
                endingText.alpha = Mathf.Clamp01((t - textDelay) / textFadeDuration);

            yield return null;
        }

        if (fadeImage != null)
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
        if (endingText != null)
            endingText.alpha = 1f;

        yield return new WaitForSecondsRealtime(textHoldDuration);
        Finish();
    }

    void Finish()
    {
        if (finished) return;
        finished = true;

        // 해피/배드 상관없이 전부 초기화 후 타이틀로
        GameReset.ResetAll();

        SceneManager.LoadScene(titleSceneName);
    }
}