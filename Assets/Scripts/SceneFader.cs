using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    [Header("같이 페이드시킬 스프라이트")]
    public SpriteFadeGroup spriteFadeGroup;

    [Header("검은 화면")]
    public Image fadeImage;
    public float fadeDuration = 1.0f;

    [Header("전환용 준비 이미지")]
    public Image transitionImage;
    public float imageFadeDuration = 0.3f;
    public float showDuration = 1.5f;

    [Header("전환 이미지 흔들림 설정")]
    public bool swayWhileShowing = true;
    public float speed = 1.5f;
    public float amount = 5.0f;

    [Header("이미지 사라진 후 검정 화면 유지")]
    public float imageFadeOutDuration = 0.3f;
    public float blackHoldDuration = 0.3f;

    [Header("스테이지 선택창")]
    public GameObject stageSelectPanel;

    private bool isTransitioning = false;

    void Start()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            StartCoroutine(FadeIn());
        }

        if (transitionImage != null)
        {
            Color imageColor = transitionImage.color;
            imageColor.a = 0f;
            transitionImage.color = imageColor;
            transitionImage.gameObject.SetActive(false);
        }

        if (stageSelectPanel != null)
        {
            stageSelectPanel.SetActive(false);
        }
    }

    // 배를 클릭하면 스테이지 선택창 표시
    public void ShowStageSelect()
    {
        if (isTransitioning) return;

        isTransitioning = true;
        StartCoroutine(FadeToStageSelect());
    }

    // 지정한 씬으로 이동
    // 입장 버튼에서 호출
    public void FadeToScene(string sceneName)
    {
        if (isTransitioning) return;

        isTransitioning = true;
        StartCoroutine(FadeOut(sceneName));
    }

    // 종료 버튼에서 호출: 화면이 어두워진 뒤 게임 종료
    public void FadeToQuit()
    {
        if (isTransitioning) return;

        isTransitioning = true;
        StartCoroutine(FadeQuit());
    }

    IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;

        float timer = 0f;
        Color color = fadeImage.color;

        color.a = 1f;
        fadeImage.color = color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(false);
    }

    // 종료용 페이드: 검게 변한 뒤 게임 종료
    IEnumerator FadeQuit()
    {
        if (fadeImage == null)
        {
            Debug.LogError("SceneFader에 Fade Image가 연결되지 않았습니다.");
            isTransitioning = false;
            yield break;
        }

        fadeImage.gameObject.SetActive(true);

        if (spriteFadeGroup != null)
        {
            StartCoroutine(spriteFadeGroup.FadeOut(fadeDuration));
        }

        float timer = 0f;
        Color color = fadeImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;

        yield return new WaitForSeconds(blackHoldDuration);

        Debug.Log("게임 종료");
#if UNITY_EDITOR
UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // 배 클릭: 검은 페이드 후 스테이지 선택창 표시
    IEnumerator FadeToStageSelect()
    {
        if (fadeImage == null)
        {
            Debug.LogError("SceneFader에 Fade Image가 연결되지 않았습니다.");
            isTransitioning = false;
            yield break;
        }

        fadeImage.gameObject.SetActive(true);

        if (spriteFadeGroup != null)
        {
            yield return StartCoroutine(
                spriteFadeGroup.FadeOut(fadeDuration)
            );
        }

        float timer = 0f;
        Color color = fadeImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;

        // 배 클릭에서는 전환 이미지를 실행하지 않음
        if (stageSelectPanel != null)
        {
            stageSelectPanel.SetActive(true);
            Debug.Log("스테이지 선택창을 표시했습니다.");
        }
        else
        {
            Debug.LogError("Stage Select Panel이 연결되지 않았습니다.");
        }

        float fadeTimer = 0f;

        while (fadeTimer < fadeDuration)
        {
            fadeTimer += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, fadeTimer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(false);

        isTransitioning = false;
    }

    // 입장 버튼: 전환 이미지 후 지정한 씬으로 이동
    IEnumerator FadeOut(string sceneName)
    {
        if (fadeImage == null)
        {
            Debug.LogError("SceneFader에 Fade Image가 연결되지 않았습니다.");
            isTransitioning = false;
            yield break;
        }

        fadeImage.gameObject.SetActive(true);

        if (spriteFadeGroup != null)
        {
            StartCoroutine(spriteFadeGroup.FadeOut(fadeDuration));
        }

        float timer = 0f;
        Color color = fadeImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;

        // 입장할 때만 전환 이미지 표시
        yield return StartCoroutine(PlayTransitionImage());

        SceneManager.LoadScene(sceneName);
    }

    IEnumerator PlayTransitionImage()
    {
        if (transitionImage == null)
        {
            Debug.LogWarning("Transition Image가 연결되지 않았습니다.");
            yield break;
        }

        Color imgColor = transitionImage.color;
        imgColor.a = 0f;
        transitionImage.color = imgColor;
        transitionImage.gameObject.SetActive(true);

        float timer = 0f;

        while (timer < imageFadeDuration)
        {
            timer += Time.deltaTime;
            imgColor.a = Mathf.Lerp(0f, 1f, timer / imageFadeDuration);
            transitionImage.color = imgColor;
            yield return null;
        }

        imgColor.a = 1f;
        transitionImage.color = imgColor;

        RectTransform rect = transitionImage.rectTransform;
        Vector2 basePos = rect.anchoredPosition;

        float shakeTimer = 0f;

        while (shakeTimer < showDuration)
        {
            shakeTimer += Time.deltaTime;

            if (swayWhileShowing)
            {
                float x = Mathf.Sin(Time.time * speed) * amount;
                float y = Mathf.Cos(Time.time * speed * 0.8f) * amount;

                rect.anchoredPosition = basePos + new Vector2(x, y);
            }

            yield return null;
        }

        rect.anchoredPosition = basePos;

        float fadeTimer = 0f;

        while (fadeTimer < imageFadeOutDuration)
        {
            fadeTimer += Time.deltaTime;
            imgColor.a = Mathf.Lerp(
                1f,
                0f,
                fadeTimer / imageFadeOutDuration
            );

            transitionImage.color = imgColor;
            yield return null;
        }

        imgColor.a = 0f;
        transitionImage.color = imgColor;
        transitionImage.gameObject.SetActive(false);

        yield return new WaitForSeconds(blackHoldDuration);
    }
}