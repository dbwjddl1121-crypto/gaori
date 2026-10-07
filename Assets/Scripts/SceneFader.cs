using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    [Header("같이 페이드시킬 스프라이트 (플레이어, 쓰레기통)")]
    public SpriteFadeGroup spriteFadeGroup;

    [Header("검은 화면")]
    public Image fadeImage; // 검은 패널의 Image 연결
    public float fadeDuration = 1.0f; // 검은 화면 페이드 시간

    [Header("전환용 준비 이미지 (선택사항)")]
    public Image transitionImage; // 검은 화면이 다 덮인 후 보여줄 이미지 (TransitionImage)
    public float imageFadeDuration = 0.3f; // 이 이미지가 나타나는 시간
    public float showDuration = 1.5f; // 이 이미지를 화면에 유지할 시간

    [Header("전환 이미지 흔들림 설정")]
    public bool swayWhileShowing = true; // 이미지 보여주는 동안 흔들지 여부
    public float speed = 1.5f;   // 흔들리는 속도
    public float amount = 5.0f;  // 흔들리는 범위(크기)

    [Header("이미지 사라진 후 검정 화면 유지")]
    public float imageFadeOutDuration = 0.3f; // 이미지가 다시 사라지는 시간
    public float blackHoldDuration = 0.3f;    // 이미지 사라진 후 순수 검정 화면을 유지할 시간

    void Start()
    {
        // 게임 시작할 때 화면이 밝아지는 효과 (선택사항)
        StartCoroutine(FadeIn());
    }

    // 외부(버튼)에서 호출할 함수
    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOut(sceneName));
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
        fadeImage.gameObject.SetActive(false); // 끝나면 패널 끄기
    }

    IEnumerator FadeOut(string sceneName)
    {
        fadeImage.gameObject.SetActive(true);

        // 검은 화면이 덮이는 것과 동시에 스프라이트도 같이 페이드아웃
        if (spriteFadeGroup != null)
            StartCoroutine(spriteFadeGroup.FadeOut(fadeDuration));

        float timer = 0f;
        Color color = fadeImage.color;

        // 1단계: 검은 화면이 서서히 덮임
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
        color.a = 1f;
        fadeImage.color = color;

        // 2단계: 완전히 까매진 후, 준비한 이미지를 서서히 보여줌
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

            // 3단계: 이 이미지를 잠시 보여주는 동안, BackgroundFloat 방식으로 살짝 흔들어줌
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

            // 원위치로 정리 (다음 씬 로드 전 깔끔하게)
            rect.anchoredPosition = basePos;

            // 4단계: 이미지를 다시 서서히 지워서 순수 검정 화면으로 되돌림
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

            // 5단계: 검정 화면만 잠깐 유지
            yield return new WaitForSeconds(blackHoldDuration);
        }

        // 완전히 어두워진(+이미지가 보인) 후 다음 씬으로 이동
        SceneManager.LoadScene(sceneName);
    }
}