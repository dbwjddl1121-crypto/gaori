using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    [Header("같이 페이드시킬 스프라이트 (플레이어, 쓰레기통)")]
    public SpriteFadeGroup spriteFadeGroup;

    [Header("검은 화면")]
    public Image fadeImage;
    public float fadeDuration = 1.0f;

    [Header("전환용 준비 이미지 (선택사항)")]
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

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    // 배 클릭용: 검은 화면으로 페이드아웃 → 선택창 켜기
    public void FadeToStageSelect()
    {
        StartCoroutine(FadeToStageSelectRoutine());
    }

    // 입장 버튼용: 이미 검은 화면 → 전환 이미지 → 씬 이동
    public void FadeToScene(string sceneName)
    {
        StartCoroutine(TransitionAndLoad(sceneName));
    }

    // 나가기용: 선택창 닫고 검은 화면 걷기
    public void CloseStageSelect()
    {
        StartCoroutine(CloseStageSelectRoutine());
    }

    IEnumerator FadeIn()
    {
        float timer = 0f;
        Color color = fadeImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
        fadeImage.gameObject.SetActive(false);
    }

    IEnumerator FadeToStageSelectRoutine()
    {
        fadeImage.gameObject.SetActive(true);

        if (spriteFadeGroup != null)
            StartCoroutine(spriteFadeGroup.FadeOut(fadeDuration));

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

        if (stageSelectPanel != null)
            stageSelectPanel.SetActive(true);
    }

    IEnumerator CloseStageSelectRoutine()
    {
        if (stageSelectPanel != null)
            stageSelectPanel.SetActive(false);

        // 투명해진 플레이어/쓰레기통 다시 보이게
        if (spriteFadeGroup != null)
            StartCoroutine(spriteFadeGroup.FadeIn(fadeDuration));

        float timer = 0f;
        Color color = fadeImage.color;

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

    IEnumerator TransitionAndLoad(string sceneName)
    {
        fadeImage.gameObject.SetActive(true);
        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;

        if (transitionImage != null)
        {
            Color imgColor = transitionImage.color;
            imgColor.a = 0f;
            transitionImage.color = imgColor;

            float t = 0f;
            while (t < imageFadeDuration)
            {
                t += Time.deltaTime;
                imgColor.a = Mathf.Lerp(0f, 1f, t / imageFadeDuration);
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

            float fadeOutTimer = 0f;
            while (fadeOutTimer < imageFadeOutDuration)
            {
                fadeOutTimer += Time.deltaTime;
                imgColor.a = Mathf.Lerp(1f, 0f, fadeOutTimer / imageFadeOutDuration);
                transitionImage.color = imgColor;
                yield return null;
            }
            imgColor.a = 0f;
            transitionImage.color = imgColor;

            yield return new WaitForSeconds(blackHoldDuration);
        }

        SceneManager.LoadScene(sceneName);
    }
}