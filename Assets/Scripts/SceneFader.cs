using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    [Header("같이 페이드시킬 스프라이트 (플레이어, 쓰레기통)")]
    public SpriteFadeGroup spriteFadeGroup;

    [Header("입장 시 플레이어 위치")]
    public Transform entrySpawnPoint;   // 문 앞 위치 (빈 오브젝트)

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

    [Header("타이틀 → 게임 전환 (시작 버튼용)")]
    public float titleFadeOutDuration = 2.0f;   // 서서히 어두워지는 시간
    public float titleHoldDuration = 0.8f;      // 완전히 검어진 뒤 유지 시간

    [Header("게임 종료 (종료 버튼용)")]
    public float quitFadeDuration = 1.0f;       // 종료 전에 어두워지는 시간

    [Header("스테이지 선택창")]
    public GameObject stageSelectPanel;

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    // 배 클릭용: 검은 화면으로 페이드아웃 → 선택창 켜기
    public void FadeToStageSelect()
    {
        StopAllCoroutines();   // 시작할 때 FadeIn이 남아 있으면 끊어서 충돌 방지
        StartCoroutine(FadeToStageSelectRoutine());
    }

    // 입장 버튼용: 이미 검은 화면 → 전환 이미지 → 씬 이동
    public void FadeToScene(string sceneName)
    {
        StartCoroutine(TransitionAndLoad(sceneName));
    }

    // 타이틀 시작 버튼용: 서서히 어두워짐 → 유지 → 씬 이동
    public void FadeToSceneSmooth(string sceneName)
    {
        TrashCollection.ResetAll();   // 새 게임 시작 시 도감 초기화
        StageLock.ResetAll();
        StopAllCoroutines();
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    // 종료 버튼용: 서서히 어두워짐 → 게임 종료
    public void FadeToQuit()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutAndQuit());
    }

    // 나가기용: 선택창 닫고 검은 화면 걷기
    public void CloseStageSelect()
    {
        StopAllCoroutines();
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
        color.a = 0f;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(false);
    }

    IEnumerator FadeOutAndLoad(string sceneName)
    {
        fadeImage.gameObject.SetActive(true);
        fadeImage.transform.SetAsLastSibling();   // 모든 UI 위로

        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;

        float timer = 0f;
        while (timer < titleFadeOutDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / titleFadeOutDuration;
            color.a = Mathf.SmoothStep(0f, 1f, t);   // 부드럽게 가속/감속
            fadeImage.color = color;
            yield return null;
        }
        color.a = 1f;
        fadeImage.color = color;

        yield return new WaitForSecondsRealtime(titleHoldDuration);

        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    IEnumerator FadeOutAndQuit()
    {
        fadeImage.gameObject.SetActive(true);
        fadeImage.transform.SetAsLastSibling();   // 모든 UI 위로

        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;

        float timer = 0f;
        while (timer < quitFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            color.a = Mathf.SmoothStep(0f, 1f, timer / quitFadeDuration);
            fadeImage.color = color;
            yield return null;
        }
        color.a = 1f;
        fadeImage.color = color;

        Debug.Log("[종료] 게임을 종료합니다.");

#if UNITY_EDITOR
        // 에디터에서는 Application.Quit()이 안 먹어서 재생을 멈춤
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    IEnumerator FadeToStageSelectRoutine()
    {
        fadeImage.gameObject.SetActive(true);

        if (spriteFadeGroup != null)
            StartCoroutine(spriteFadeGroup.FadeOut(fadeDuration));

        float timer = 0f;
        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;

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
        {
            stageSelectPanel.SetActive(true);
            stageSelectPanel.transform.SetAsLastSibling();   // 검은 화면 위로 확실히
        }
        else
        {
            Debug.LogWarning("SceneFader: stageSelectPanel이 연결되지 않았어요!");
        }
    }

    IEnumerator CloseStageSelectRoutine()
    {
        Debug.Log("[Fader] 닫기 시작 / timeScale = " + Time.timeScale);

        fadeImage.gameObject.SetActive(true);

        if (stageSelectPanel != null)
            stageSelectPanel.SetActive(false);

        Debug.Log("[Fader] 패널 끄기 완료");

        if (spriteFadeGroup != null)
            StartCoroutine(spriteFadeGroup.FadeIn(fadeDuration));

        float timer = 0f;
        Color color = fadeImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            color.a = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(false);

        Debug.Log("[Fader] 닫기 완료");
    }

    IEnumerator TransitionAndLoad(string sceneName)
    {
        // 스테이지창이 전환 이미지를 가리지 않게 입장하는 순간 닫기
        if (stageSelectPanel != null)
            stageSelectPanel.SetActive(false);

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

        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}