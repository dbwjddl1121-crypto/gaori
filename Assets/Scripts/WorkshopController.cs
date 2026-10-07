using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WorkshopController : MonoBehaviour
{
    public GameObject workshopInteriorPanel;
    public GameObject blackScreenPanel;

    [Header("배경과 같이 페이드시킬 스프라이트 (플레이어, 쓰레기통)")]
    public SpriteFadeGroup spriteFadeGroup;

    private CanvasGroup interiorCanvasGroup;
    private CanvasGroup blackCanvasGroup;

    [Header("페이드 속도 설정")]
    public float blackFadeDuration = 0.25f;
    public float interiorFadeDuration = 0.3f;
    [Range(0f, 1f)] public float maxBlackAlpha = 0.75f;

    void Awake()
    {
        if (workshopInteriorPanel != null)
        {
            interiorCanvasGroup = workshopInteriorPanel.GetComponent<CanvasGroup>();
            if (interiorCanvasGroup == null)
                interiorCanvasGroup = workshopInteriorPanel.AddComponent<CanvasGroup>();
        }

        if (blackScreenPanel != null)
        {
            blackCanvasGroup = blackScreenPanel.GetComponent<CanvasGroup>();
            if (blackCanvasGroup == null)
                blackCanvasGroup = blackScreenPanel.AddComponent<CanvasGroup>();
        }
    }

    // 배경 alpha와 스프라이트 alpha를 항상 같이 설정
    private void SetInteriorAlpha(float a)
    {
        if (interiorCanvasGroup != null) interiorCanvasGroup.alpha = a;
        if (spriteFadeGroup != null) spriteFadeGroup.SetAlpha(a);
    }

    public void EnterWorkshop()
    {
        StopAllCoroutines();
        StartCoroutine(FadeInWorkshopSequence());
    }

    public void ExitWorkshop()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutWorkshopSequence());
    }

    IEnumerator FadeInWorkshopSequence()
    {
        if (blackScreenPanel != null)
        {
            blackScreenPanel.SetActive(true);
            blackCanvasGroup.alpha = 0f;
            blackCanvasGroup.blocksRaycasts = true; // 뒤쪽 클릭 막기
        }

        if (workshopInteriorPanel != null)
        {
            workshopInteriorPanel.SetActive(false);
            SetInteriorAlpha(0f);
        }

        // 1단계: 검은 화면이 서서히 어두워짐
        float timer = 0f;
        while (timer < blackFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            blackCanvasGroup.alpha = Mathf.Clamp01(timer / blackFadeDuration) * maxBlackAlpha;
            yield return null;
        }
        blackCanvasGroup.alpha = maxBlackAlpha;

        // 2단계: 작업장 등장 (알파 0 상태에서 켜기)
        if (workshopInteriorPanel != null) workshopInteriorPanel.SetActive(true);

        timer = 0f;
        while (timer < interiorFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            SetInteriorAlpha(Mathf.Clamp01(timer / interiorFadeDuration));
            yield return null;
        }
        SetInteriorAlpha(1f);
    }

    IEnumerator FadeOutWorkshopSequence()
    {
        float timer = 0f;
        float startInteriorAlpha = interiorCanvasGroup != null ? interiorCanvasGroup.alpha : 1f;

        // 1단계: 작업장이 사라짐 (스프라이트도 같이)
        while (timer < interiorFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            SetInteriorAlpha(Mathf.Lerp(startInteriorAlpha, 0f, timer / interiorFadeDuration));
            yield return null;
        }

        SetInteriorAlpha(0f);
        if (workshopInteriorPanel != null) workshopInteriorPanel.SetActive(false);

        // 2단계: 검은 화면이 걷힘
        timer = 0f;
        float startBlackAlpha = blackCanvasGroup != null ? blackCanvasGroup.alpha : maxBlackAlpha;
        while (timer < blackFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            blackCanvasGroup.alpha = Mathf.Lerp(startBlackAlpha, 0f, timer / blackFadeDuration);
            yield return null;
        }

        blackCanvasGroup.alpha = 0f;
        blackCanvasGroup.blocksRaycasts = false;
        if (blackScreenPanel != null) blackScreenPanel.SetActive(false);
    }
}