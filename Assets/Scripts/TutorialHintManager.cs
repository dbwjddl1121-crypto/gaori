using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class TutorialHintManager : MonoBehaviour
{
    [Header("연결 (TutorialHint 오브젝트에 붙이면 자동으로 찾아요)")]
    public CanvasGroup hintGroup;
    public TMP_Text hintText;

    [Header("설정")]
    public float startDelay = 1.5f;        // 바다에 들어간 뒤 첫 힌트가 뜨기까지 기다리는 시간
    public float fadeTime = 0.4f;          // 나타나고 사라지는 시간
    public float maxShowTime = 10f;        // 이 시간이 지나면 행동을 안 해도 힌트가 사라짐
    public float fishHintShowTime = 20f;   // 물고기 힌트는 3번 쳐야 할 수 있어서 더 오래 보여줌
    public float trashHintDistance = 30f;  // 쓰레기에 이만큼 가까워지면 줍기 힌트가 뜸
    public float fishHintDistance = 30f;   // 물고기에 이만큼 가까워지면 잡기 힌트가 뜸

    [Header("테스트용: 체크하면 본 기록을 무시하고 매번 보여줌 (완성 후 꺼주세요)")]
    public bool alwaysShowForTest = true;

    const string KEY_MOVE = "Tut_Move";
    const string KEY_TRASH = "Tut_Trash";
    const string KEY_INVENTORY = "Tut_Inventory";
    const string KEY_QUICK = "Tut_Quick";
    const string KEY_FISH = "Tut_Fish";
    const string KEY_EXIT = "Tut_Exit";

    private string curKo = "", curEn = "", curVi = "";
    private Transform player;
    private Inventory inventory;

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
        inventory = FindFirstObjectByType<Inventory>();

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

    // 튜토리얼 전체 흐름
    IEnumerator Flow()
    {
        yield return new WaitForSecondsRealtime(startDelay);

        // 1. 이동 (WASD)
        if (!IsDone(KEY_MOVE))
        {
            yield return ShowHint(
                "W A S D 로 이동할 수 있어요",
                "Move with W A S D",
                "Di chuyển bằng W A S D",
                () => Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A)
                   || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D),
                maxShowTime);
            MarkDone(KEY_MOVE);
        }

        // 2. 쓰레기 줍기 (좌클릭)
        int trashBaseline = RunStats.trashCount;

        if (!IsDone(KEY_TRASH))
        {
            // 쓰레기에 가까워지거나, 힌트 없이 먼저 주울 때까지 기다림
            while (!TrashNearby() && RunStats.trashCount <= trashBaseline)
                yield return new WaitForSecondsRealtime(0.3f);

            if (RunStats.trashCount <= trashBaseline)
            {
                yield return ShowHint(
                    "쓰레기를 좌클릭해서 주워 보세요",
                    "Left click the trash to pick it up",
                    "Nhấp chuột trái vào rác để nhặt",
                    () => RunStats.trashCount > trashBaseline,
                    maxShowTime);
            }
            MarkDone(KEY_TRASH);
        }

        // 쓰레기를 한 개라도 주울 때까지 기다림 (이미 주웠다면 바로 통과)
        while (RunStats.trashCount <= trashBaseline && !IsDone(KEY_INVENTORY))
            yield return new WaitForSecondsRealtime(0.3f);

        // 3. 인벤토리 열기 (E)
        if (!IsDone(KEY_INVENTORY))
        {
            yield return new WaitForSecondsRealtime(0.8f);
            yield return ShowHint(
                "E 키로 인벤토리를 열 수 있어요",
                "Press E to open your inventory",
                "Nhấn E để mở túi đồ",
                () => Input.GetKeyDown(KeyCode.E),
                maxShowTime);
            MarkDone(KEY_INVENTORY);
        }

        // 인벤토리가 닫힐 때까지 기다림
        while (InventoryOpen())
            yield return new WaitForSecondsRealtime(0.3f);

        // 4. 퀵슬롯 선택 + 아이템 사용 (1~8, Q)
        if (!IsDone(KEY_QUICK))
        {
            yield return new WaitForSecondsRealtime(0.5f);
            yield return ShowHint(
                "숫자키 1~8로 칸을 고르고,\nQ 키로 아이템을 써요",
                "Pick a slot with 1~8, then press Q to use the item",
                "Chọn ô bằng phím 1~8, rồi nhấn Q để dùng vật phẩm",
                () => Input.GetKeyDown(KeyCode.Q),
                maxShowTime);
            MarkDone(KEY_QUICK);
        }

        // 5. 물고기 잡기 (우클릭)
        int fishBaseline = RunStats.fishCaught;

        if (!IsDone(KEY_FISH))
        {
            // 물고기에 가까워지거나, 힌트 없이 먼저 잡을 때까지 기다림
            while (!FishNearby() && RunStats.fishCaught <= fishBaseline)
                yield return new WaitForSecondsRealtime(0.3f);

            if (RunStats.fishCaught <= fishBaseline)
            {
                yield return ShowHint(
                    "물고기를 우클릭해서 잡아 보세요",
                    "Right click the fish to catch it",
                    "Nhấp chuột phải vào cá để bắt",
                    () => RunStats.fishCaught > fishBaseline,
                    fishHintShowTime);
            }
            MarkDone(KEY_FISH);
        }

        // 6. 바다에서 나가기 (B) - 물고기 힌트가 끝나면 바로 이어서 표시
        if (!IsDone(KEY_EXIT))
        {
            yield return new WaitForSecondsRealtime(0.8f);
            yield return ShowHint(
                "B 키를 누르면 바다에서 나갈 수 있어요",
                "Press B to leave the sea",
                "Nhấn B để rời khỏi biển",
                () => Input.GetKeyDown(KeyCode.B),
                maxShowTime);
            MarkDone(KEY_EXIT);
        }
    }

    bool InventoryOpen()
    {
        return inventory != null
            && inventory.inventoryPanel != null
            && inventory.inventoryPanel.activeSelf;
    }

    // Player를 못 찾았으면 다시 찾아서 돌려줌
    Transform GetPlayer()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        return player;
    }

    // 플레이어가 쓰레기에 가까이 있는지 확인
    bool TrashNearby()
    {
        Transform pl = GetPlayer();
        if (pl == null) return false;

        TrashClickPickup[] trashes =
            FindObjectsByType<TrashClickPickup>(FindObjectsSortMode.None);

        foreach (TrashClickPickup t in trashes)
        {
            if (t == null) continue;
            if (Vector2.Distance(pl.position, t.transform.position) <= trashHintDistance)
                return true;
        }
        return false;
    }

    // 플레이어가 물고기에 가까이 있는지 확인
    bool FishNearby()
    {
        Transform pl = GetPlayer();
        if (pl == null) return false;

        PixelFishSmooth[] fishes =
            FindObjectsByType<PixelFishSmooth>(FindObjectsSortMode.None);

        foreach (PixelFishSmooth f in fishes)
        {
            if (f == null) continue;
            if (Vector2.Distance(pl.position, f.transform.position) <= fishHintDistance)
                return true;
        }
        return false;
    }

    // 힌트 하나를 띄우고, 조건을 만족하거나 시간이 지나면 사라지게 함
    IEnumerator ShowHint(string ko, string en, string vi, Func<bool> doneCondition, float showTime)
    {
        curKo = ko; curEn = en; curVi = vi;
        RefreshText();

        yield return Fade(0f, 1f);

        float timer = 0f;
        while (timer < showTime)
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