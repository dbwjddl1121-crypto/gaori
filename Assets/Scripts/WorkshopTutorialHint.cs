using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class WorkshopTutorialHint : MonoBehaviour
{
    [Header("연결")]
    public CanvasGroup hintGroup;
    public TMP_Text hintText;

    [Tooltip("상점 창 오브젝트 (책상을 클릭하면 켜지는 것). 연결하면 상점이 열렸을 때 힌트가 사라져요")]
    public GameObject shopPanel;

    [Header("설정")]
    public float startDelay = 1f;          // 입장 연출이 끝난 뒤 첫 힌트까지 기다리는 시간
    public float fadeTime = 0.4f;
    public float maxShowTime = 12f;        // 이 시간이 지나면 행동을 안 해도 힌트가 사라짐
    public float binHintDistance = 6f;     // 쓰레기통에 이만큼 가까워지면 힌트가 뜸

    [Header("테스트용: 체크하면 본 기록을 무시하고 매번 보여줌 (완성 후 꺼주세요)")]
    public bool alwaysShowForTest = true;

    const string KEY_BIN = "Tut_W_Bin";
    const string KEY_SHOP = "Tut_W_Shop";
    const string KEY_TAB = "Tut_W_Tab";

    private string curKo = "", curEn = "", curVi = "";
    private PlayerMove playerMove;

    void Awake()
    {
        if (hintGroup == null) hintGroup = GetComponent<CanvasGroup>();
        if (hintGroup != null)
        {
            hintGroup.alpha = 0f;
            hintGroup.interactable = false;
            hintGroup.blocksRaycasts = false; // 힌트가 마우스 클릭을 막지 않게
        }
    }

    void Start()
    {
        playerMove = FindFirstObjectByType<PlayerMove>();

        Localization.OnLanguageChanged += RefreshText;
        StartCoroutine(Flow());
    }

    void OnDestroy()
    {
        Localization.OnLanguageChanged -= RefreshText;
    }

    void RefreshText()
    {
        if (hintText != null)
            hintText.text = Localization.Pick(curKo, curEn, curVi);
    }

    bool IsDone(string key)
    {
        if (alwaysShowForTest) return false;
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    void MarkDone(string key)
    {
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }

    bool ShopOpen()
    {
        return shopPanel != null && shopPanel.activeInHierarchy;
    }

    IEnumerator Flow()
    {
        // 입장 연출이 끝나 플레이어를 움직일 수 있을 때까지 기다림
        while (playerMove != null && !playerMove.enabled)
            yield return new WaitForSecondsRealtime(0.2f);

        yield return new WaitForSecondsRealtime(startDelay);

        // 1. 쓰레기통 좌클릭 (쓰레기통 가까이 갔을 때)
        int recycleBaseline = TrashCanController.RecycleCount;

        if (!IsDone(KEY_BIN))
        {
            while (!BinNearby() && TrashCanController.RecycleCount <= recycleBaseline)
                yield return new WaitForSecondsRealtime(0.3f);

            if (TrashCanController.RecycleCount <= recycleBaseline)
            {
                yield return ShowHint(
                    "숫자키로 쓰레기를 고르고\n쓰레기통을 좌클릭해요",
                    "Select trash with a number key,\nthen left click the bin",
                    "Chọn rác bằng phím số,\nrồi nhấp chuột trái vào thùng rác",
                    () => TrashCanController.RecycleCount > recycleBaseline);
            }
            MarkDone(KEY_BIN);
        }

        // 2. 상점 (책상 클릭)
        if (!IsDone(KEY_SHOP))
        {
            yield return new WaitForSecondsRealtime(0.8f);

            if (!ShopOpen())
            {
                yield return ShowHint(
                    "책상을 클릭하면 상점이 열려요",
                    "Click the desk to open the shop",
                    "Nhấp vào bàn để mở cửa hàng",
                    () => ShopOpen());
            }
            MarkDone(KEY_SHOP);
        }

        // 상점이 열려 있는 동안은 기다림
        while (ShopOpen())
            yield return new WaitForSecondsRealtime(0.3f);

        // 3. TAB으로 퀵슬롯 줄 바꾸기
        if (!IsDone(KEY_TAB))
        {
            yield return new WaitForSecondsRealtime(0.8f);
            yield return ShowHint(
                "TAB 키로 퀵슬롯 줄을 바꿀 수 있어요",
                "Press TAB to switch the quick slot row",
                "Nhấn TAB để đổi hàng ô nhanh",
                () => Input.GetKeyDown(KeyCode.Tab));
            MarkDone(KEY_TAB);
        }
    }

    // 플레이어가 분리수거통 가까이 있는지 확인
    bool BinNearby()
    {
        if (playerMove == null)
            playerMove = FindFirstObjectByType<PlayerMove>();
        if (playerMove == null) return false;

        TrashCanController[] bins =
            FindObjectsByType<TrashCanController>(FindObjectsSortMode.None);

        foreach (TrashCanController b in bins)
        {
            if (b == null) continue;
            if (Vector2.Distance(playerMove.transform.position, b.transform.position) <= binHintDistance)
                return true;
        }
        return false;
    }

    IEnumerator ShowHint(string ko, string en, string vi, Func<bool> doneCondition)
    {
        curKo = ko; curEn = en; curVi = vi;
        RefreshText();

        yield return Fade(0f, 1f);

        float timer = 0f;
        while (timer < maxShowTime)
        {
            if (doneCondition != null && doneCondition()) break;
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        yield return Fade(1f, 0f);
    }

    IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            if (hintGroup != null)
                hintGroup.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }
        if (hintGroup != null) hintGroup.alpha = to;
    }
}