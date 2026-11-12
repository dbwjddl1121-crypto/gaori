using UnityEngine;
using System.Collections;

// 분리수거통 종류
public enum TrashCanType
{
    Glass,      // 유리
    Plastic,    // 플라스틱
    Vinyl,      // 비닐
    Can,        // 캔
    General     // 일반쓰레기
}

public class TrashCanController : MonoBehaviour
{
    [Header("분리수거통 설정")]
    public TrashCanType canType;
    public QuickSlotBar quickSlotBar;
    public InventoryManager inventory;

    [Header("거리 설정")]
    public float interactDistance = 2f;   // 이 거리 안에 들어와야 터치 가능
    private Transform player;

    [Header("애니메이션 설정")]
    private Animator anim;

    [Header("오답 피드백 설정")]
    public SpriteRenderer spriteRenderer;   // 분리수거통 스프라이트 (비워두면 자동으로 찾음)
    public Color wrongColor = new Color(1f, 0.3f, 0.3f, 1f); // 빨갛게 변할 색
    public int flashCount = 3;              // 깜빡이는 횟수
    public float flashInterval = 0.08f;     // 한 번 깜빡이는 시간(초)
    public float shakeStrength = 0.1f;      // 좌우로 흔들리는 폭
    public float shakeDuration = 0.3f;      // 흔들리는 총 시간(초)

    private Color originalColor;
    private Vector3 originalPos;
    private Coroutine flashCoroutine;
    private Coroutine shakeCoroutine;

    [Header("캐시 보상 설정")]
    public int noMiniGameCashReward = 100; // 미니게임 없는 쓰레기 분리수거 시 지급할 캐시

    private void Start()
    {
        anim = GetComponent<Animator>();

        PlayerMove playerMove = FindFirstObjectByType<PlayerMove>();
        if (playerMove != null)
            player = playerMove.transform;

        if (quickSlotBar == null)
            quickSlotBar = FindFirstObjectByType<QuickSlotBar>();

        if (inventory == null)
            inventory = FindFirstObjectByType<InventoryManager>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        originalPos = transform.localPosition;
    }

    private void OnMouseDown()
    {
        TryRecycle();
    }

    public void TryRecycle()
    {
        // 플레이어가 너무 멀면 터치 불가
        if (player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);

            if (dist > interactDistance)
            {
                NoticeUI.Show(
                    "분리수거통이 너무 멀리 있습니다.",
                    "The recycling bin is too far away.",
                    "Thùng phân loại rác ở quá xa."
                );
                return;
            }
        }

        // 미니게임 중에는 다른 쓰레기를 버릴 수 없음
        if (MiniGameManager.Instance != null && MiniGameManager.Instance.IsPlaying)
            return;

        // 퀵슬롯 선택 여부 확인
        if (quickSlotBar == null || !quickSlotBar.IsItemSelected())
        {
            Debug.Log("[실패] 퀵슬롯이 선택되지 않았습니다. 숫자키를 먼저 눌러주세요.");
            return;
        }

        // 선택한 아이템 확인
        Item selectedItem = quickSlotBar.GetSelectedItem();

        if (selectedItem == null)
        {
            Debug.Log("[실패] 선택된 칸에 아이템이 없습니다.");
            return;
        }

        // 인벤토리 연결 확인
        if (inventory == null)
        {
            Debug.LogError("[실패] InventoryManager를 찾을 수 없습니다.");
            return;
        }

        // 올바른 분리수거통인지 확인
        if (!IsCorrectTrash(selectedItem, canType))
        {
            NoticeUI.Show(
                "잘못된 분리수거통입니다.",
                "Wrong recycling bin.",
                "Sai thùng phân loại rác."
            );
            PlayWrongFeedback(); // ★ 잘못 눌렀을 때 빨간 점멸 + 쉐이크 피드백
            return;
        }

        // 선택한 퀵슬롯 번호 확인
        int selectedIndex = quickSlotBar.GetSelectedIndex();

        if (selectedIndex < 0)
        {
            Debug.LogWarning("[실패] 선택한 퀵슬롯 번호가 올바르지 않습니다.");
            return;
        }

        string recycledItemName = selectedItem.itemName;

        // 분리수거 성공
        Debug.Log($"[성공] {recycledItemName}을(를) 올바르게 분리수거했습니다!");

        // 뚜껑 애니메이션 재생
        if (anim != null)
        {
            anim.SetTrigger("IsOpen");
            anim.SetBool("IsOpen", true);
        }

        // 실제 인벤토리 아이템 제거 및 저장
        inventory.RemoveItemAt(selectedIndex);

        // 퀵슬롯 UI 갱신
        quickSlotBar.Refresh();

        // 미니게임이 있는 쓰레기면 미니게임 시작, 없으면 바로 캐시 지급
        if (MiniGameManager.HasMiniGame(recycledItemName))
        {
            if (MiniGameManager.Instance != null)
            {
                MiniGameManager.Instance.StartMiniGame(recycledItemName);
            }
            else
            {
                Debug.LogWarning(
                    "[미니게임 오류] 씬에 MiniGameManager가 없습니다. " +
                    "빈 오브젝트에 MiniGameManager를 붙여 주세요."
                );
            }
        }
        else
        {
            // ★ 미니게임이 없는 쓰레기는 분리수거 즉시 캐시 지급
            if (CashManager.Instance != null)
            {
                CashManager.Instance.AddCash(noMiniGameCashReward);
                Debug.Log($"[캐시 지급] {recycledItemName} 분리수거로 {noMiniGameCashReward}캐시를 받았습니다!");
            }
            else
            {
                Debug.LogWarning("[캐시 오류] 씬에 CashManager가 없습니다.");
            }
        }
    }

    // ★ 오답 피드백: 빨간 점멸 + 좌우 흔들림
    private void PlayWrongFeedback()
    {
        // 연출이 돌고 있지 않을 때만 "지금 상태"를 원래 값으로 저장
        // (페이드 도중에 저장된 투명한 색/어긋난 위치를 쓰지 않기 위해)
        if (flashCoroutine == null && spriteRenderer != null)
            originalColor = spriteRenderer.color;

        if (shakeCoroutine == null)
            originalPos = transform.localPosition;

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);

        if (spriteRenderer != null)
            flashCoroutine = StartCoroutine(FlashRedRoutine());

        shakeCoroutine = StartCoroutine(ShakeRoutine());
    }

    private IEnumerator FlashRedRoutine()
    {
        // 빨간색이 되어도 투명도는 원래 값 그대로 유지
        Color flashColor = wrongColor;
        flashColor.a = originalColor.a;

        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashInterval);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashInterval);
        }
        spriteRenderer.color = originalColor;
        flashCoroutine = null;
    }

    private IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float offsetX = Mathf.Sin(elapsed * 60f) * shakeStrength;
            transform.localPosition = originalPos + new Vector3(offsetX, 0f, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPos;
        shakeCoroutine = null;
    }

    // 쓰레기 이름은 구분용이라 한국어 그대로 둬야 해요. 번역하면 안 돼요.
    private bool IsCorrectTrash(Item item, TrashCanType targetCan)
    {
        if (item == null) return false;

        string name = item.itemName;

        switch (targetCan)
        {
            case TrashCanType.Glass:
                return name == "사이다병"
                    || name == "유리 공병"
                    || name == "맥주병"
                    || name == "음료수병";

            case TrashCanType.Plastic:
                return name == "생수병";

            case TrashCanType.Vinyl:
                return name == "비닐봉지"
                    || name == "과자봉지"
                    || name == "라면 봉지";

            case TrashCanType.Can:
                return name == "참치캔"
                    || name == "통조림 캔"
                    || name == "콜라캔";

            case TrashCanType.General:
                return name == "종이컵"
                    || name == "피자박스"
                    || name == "깨진 유리";
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}