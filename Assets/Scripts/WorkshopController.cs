using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WorkshopController : MonoBehaviour
{
    public GameObject workshopInteriorPanel;
    public GameObject blackScreenPanel;

    private CanvasGroup interiorCanvasGroup;
    private CanvasGroup blackCanvasGroup;
    private GraphicRaycaster blackRaycaster;

    [Header("페이드 속도 설정")]
    public float blackFadeDuration = 0.25f; // 1단계: 검은 화면이 어두워지는 시간
    public float interiorFadeDuration = 0.3f; // 2단계: 작업장 창이 나타나는 시간
    [Range(0f, 1f)] public float maxBlackAlpha = 0.75f; // 검은 화면의 어두운 정도

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

            blackRaycaster = blackScreenPanel.GetComponent<GraphicRaycaster>();
            if (blackRaycaster == null)
                blackRaycaster = blackScreenPanel.AddComponent<GraphicRaycaster>();
        }
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

    // 🌟 순차적 등장 연출 (검은 화면 먼저 -> 작업장 나중에)
    IEnumerator FadeInWorkshopSequence()
    {
        // 초기화
        if (blackScreenPanel != null)
        {
            blackScreenPanel.SetActive(true);
            if (blackRaycaster != null) blackRaycaster.enabled = true;
        }
        if (workshopInteriorPanel != null)
        {
            workshopInteriorPanel.SetActive(false); // 오타 수정 완료
            if (interiorCanvasGroup != null) interiorCanvasGroup.alpha = 0f;
        }

        if (blackCanvasGroup != null) blackCanvasGroup.alpha = 0f;

        // 1단계: 검은 화면이 먼저 서서히 어두워짐
        float timer = 0f;
        while (timer < blackFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / blackFadeDuration);
            if (blackCanvasGroup != null) blackCanvasGroup.alpha = progress * maxBlackAlpha;
            yield return null;
        }
        if (blackCanvasGroup != null) blackCanvasGroup.alpha = maxBlackAlpha;

        // 2단계: 검은 화면이 뜬 상태에서 작업장 창이 서서히 나타남
        if (workshopInteriorPanel != null) workshopInteriorPanel.SetActive(true);
        timer = 0f;
        while (timer < interiorFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / interiorFadeDuration);
            if (interiorCanvasGroup != null) interiorCanvasGroup.alpha = progress;
            yield return null;
        }
        if (interiorCanvasGroup != null) interiorCanvasGroup.alpha = 1f;
    }

    // 🌟 순차적 퇴장 연출 (작업장 먼저 사라짐 -> 검은 화면 사라짐)
    IEnumerator FadeOutWorkshopSequence()
    {
        float timer = 0f;
        float startInteriorAlpha = interiorCanvasGroup != null ? interiorCanvasGroup.alpha : 1f;

        // 1단계: 작업장 창이 먼저 서서히 사라짐
        while (timer < interiorFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / interiorFadeDuration);
            if (interiorCanvasGroup != null) interiorCanvasGroup.alpha = Mathf.Lerp(startInteriorAlpha, 0f, progress);
            yield return null;
        }

        if (interiorCanvasGroup != null) interiorCanvasGroup.alpha = 0f;
        if (workshopInteriorPanel != null) workshopInteriorPanel.SetActive(false);

        // 2단계: 작업장이 사라진 후, 검은 화면이 서서히 걷힘
        timer = 0f;
        float startBlackAlpha = blackCanvasGroup != null ? blackCanvasGroup.alpha : maxBlackAlpha;
        while (timer < blackFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / blackFadeDuration);
            if (blackCanvasGroup != null) blackCanvasGroup.alpha = Mathf.Lerp(startBlackAlpha, 0f, progress);
            yield return null;
        }

        if (blackCanvasGroup != null) blackCanvasGroup.alpha = 0f;
        if (blackRaycaster != null) blackRaycaster.enabled = false;
        if (blackScreenPanel != null) blackScreenPanel.SetActive(false);
    }
}