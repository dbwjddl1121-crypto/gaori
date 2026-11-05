using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class WorkshopController : MonoBehaviour
{
    public GameObject workshopInteriorPanel;
    public GameObject blackScreenPanel;

    [Header("배경과 같이 페이드시킬 스프라이트 (플레이어, 쓰레기통)")]
    public SpriteFadeGroup spriteFadeGroup;

    [Header("입장 시 플레이어 위치")]
    public Transform entrySpawnPoint;   // 문 앞 위치 (빈 오브젝트)

    private CanvasGroup interiorCanvasGroup;
    private CanvasGroup blackCanvasGroup;

    [Header("페이드 속도 설정")]
    public float blackFadeDuration = 0.25f;
    public float interiorFadeDuration = 0.3f;
    [Range(0f, 1f)] public float maxBlackAlpha = 0.75f;

    // 게임 실행 후 첫 입장 여부 (씬을 다시 불러와도 유지됨)
    private static bool hasEnteredOnce = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic()
    {
        hasEnteredOnce = false;
    }

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

    private void SetInteriorAlpha(float a)
    {
        if (interiorCanvasGroup != null) interiorCanvasGroup.alpha = a;
        if (spriteFadeGroup != null) spriteFadeGroup.SetAlpha(a);
    }

    private void MovePlayerToEntry()
    {
        // 게임 실행 후 첫 입장은 기존 입장 연출을 살리기 위해 위치를 옮기지 않음
        if (!hasEnteredOnce)
        {
            hasEnteredOnce = true;
            return;
        }

        if (entrySpawnPoint == null) return;

        PlayerMove pm = FindFirstObjectByType<PlayerMove>();
        if (pm == null) return;

        Vector3 pos = entrySpawnPoint.position;
        pos.z = pm.transform.position.z;   // 플레이어 z값은 유지
        pm.transform.position = pos;
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
        MovePlayerToEntry();

        timer = 0f;
        while (timer < interiorFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            SetInteriorAlpha(Mathf.Clamp01(timer / interiorFadeDuration));
            yield return null;
        }
        SetInteriorAlpha(1f);

        // 3단계: 검은 화면을 다시 걷어줌
        timer = 0f;
        while (timer < blackFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            blackCanvasGroup.alpha = Mathf.Lerp(maxBlackAlpha, 0f, timer / blackFadeDuration);
            yield return null;
        }
        blackCanvasGroup.alpha = 0f;
        blackCanvasGroup.blocksRaycasts = false;
        if (blackScreenPanel != null) blackScreenPanel.SetActive(false);
    }

    IEnumerator FadeOutWorkshopSequence()
    {
        if (blackScreenPanel != null)
        {
            blackScreenPanel.SetActive(true);
            blackCanvasGroup.alpha = 0f;
            blackCanvasGroup.blocksRaycasts = true;
        }

        float timer = 0f;
        while (timer < blackFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            blackCanvasGroup.alpha = Mathf.Clamp01(timer / blackFadeDuration) * maxBlackAlpha;
            yield return null;
        }
        blackCanvasGroup.alpha = maxBlackAlpha;

        float startInteriorAlpha = interiorCanvasGroup != null ? interiorCanvasGroup.alpha : 1f;

        timer = 0f;
        while (timer < interiorFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            SetInteriorAlpha(Mathf.Lerp(startInteriorAlpha, 0f, timer / interiorFadeDuration));
            yield return null;
        }

        SetInteriorAlpha(0f);
        if (workshopInteriorPanel != null) workshopInteriorPanel.SetActive(false);

        timer = 0f;
        while (timer < blackFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            blackCanvasGroup.alpha = Mathf.Lerp(maxBlackAlpha, 0f, timer / blackFadeDuration);
            yield return null;
        }

        blackCanvasGroup.alpha = 0f;
        blackCanvasGroup.blocksRaycasts = false;
        if (blackScreenPanel != null) blackScreenPanel.SetActive(false);
    }
}