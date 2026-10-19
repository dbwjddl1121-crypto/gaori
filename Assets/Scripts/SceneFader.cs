
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

        if (stageSelectPanel != null)
        {
            stageSelectPanel.SetActive(false);
        }
    }

    // 기존 버튼의 씬 이동 기능 유지
    public void FadeToScene(string sceneName)
    {
        if (isTransitioning) return;

        isTransitioning = true;
        StartCoroutine(FadeOut(sceneName));
    }

    // 배를 클릭했을 때 스테이지 선택창을 표시
    public void ShowStageSelect()
    {
        if (isTransitioning) return;

        isTransitioning = true;
        StartCoroutine(FadeToStageSelect());
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
            StartCoroutine(spriteFadeGroup.FadeOut(fadeDuration));
        }

        // 1. 검은 화면으로 전환
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

        // 2. 전환 이미지 연출
        yield return StartCoroutine(PlayTransitionImage());

        // 3. 스테이지 선택창 표시
        if (stageSelectPanel != null)
        {
            stageSelectPanel.SetActive(true);
            Debug.Log("스테이지 선택창을 표시했습니다.");
        }
        else
        {
            Debug.LogError(
                "Stage Select Panel이 연결되지 않았습니다. " +
                "Inspector에서 연결해 주세요."
            );
        }

        // 4. 검은 화면 제거
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

        yield return StartCoroutine(PlayTransitionImage());

        SceneManager.LoadScene(sceneName);
    }

    IEnumerator PlayTransitionImage()
    {
        if (transitionImage == null)
        {
            yield break;
        }

        Color imgColor = transitionImage.color;
        imgColor.a = 0f;
        transitionImage.color = imgColor;
        transitionImage.gameObject.SetActive(true);

        // 1. 전환 이미지 나타내기
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

        // 2. 이미지 흔들림
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

        // 3. 전환 이미지 사라지기
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

        // 4. 검은 화면 유지
        yield return new WaitForSeconds(blackHoldDuration);
    }
}
