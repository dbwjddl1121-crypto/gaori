
using UnityEngine;

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

    [Header("애니메이션 설정")]
    private Animator anim;

    [Header("미니게임 설정")]
    public GameObject miniGamePanel;

    private void Start()
    {
        anim = GetComponent<Animator>();

        if (quickSlotBar == null)
            quickSlotBar = FindFirstObjectByType<QuickSlotBar>();

        if (inventory == null)
            inventory = FindFirstObjectByType<InventoryManager>();

        // 게임 시작 시 미니게임 패널 숨기기
        if (miniGamePanel != null)
            miniGamePanel.SetActive(false);
    }

    private void OnMouseDown()
    {
        TryRecycle();
    }

    public void TryRecycle()
    {
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
            Debug.Log($"[실패] {selectedItem.itemName}은(는) 이 분리수거통({canType})에 버릴 수 없습니다!");
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

        // 생수병을 올바르게 버렸을 때 미니게임 표시
        if (recycledItemName == "생수병")
        {
            if (miniGamePanel != null)
            {
                miniGamePanel.SetActive(true);
                Debug.Log("[미니게임] 생수병 라벨 제거 미니게임 시작!");
            }
            else
            {
                Debug.LogWarning(
                    "[미니게임 오류] MiniGamePanel이 연결되지 않았습니다. " +
                    "Inspector에서 연결해 주세요."
                );
            }
        }
    }

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
                return name == "비닐봉지";

            case TrashCanType.Can:
                return name == "참치캔"
                    || name == "통조림 캔"
                    || name == "콜라캔";

            case TrashCanType.General:
                return name == "종이컵"
                    || name == "피자박스"
                    || name == "깨진 유리"
                    || name == "과자봉지"
                    || name == "라면 봉지";
        }

        return false;
    }
}
