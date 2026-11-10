using UnityEngine;

// 테스트용: 단축키로 인벤토리에 쓰레기 추가 (에디터에서만 동작)
public class InventoryDebug : MonoBehaviour
{
    [Header("테스트용 아이템 아이콘 (여러 개 한꺼번에 드래그해서 넣으세요)")]
    public Sprite[] testIcons;

    [Header("단축키")]
    public KeyCode addNextKey = KeyCode.F1;   // 목록의 다음 아이템 1개 추가
    public KeyCode addAllKey = KeyCode.F2;    // 목록 전체를 빈 칸이 찰 때까지 추가
    public KeyCode clearKey = KeyCode.F3;     // 인벤토리 전부 비우기

    private InventoryManager inventory;
    private int nextIndex = 0;

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(addNextKey)) AddNext();
        if (Input.GetKeyDown(addAllKey)) AddAll();
        if (Input.GetKeyDown(clearKey)) ClearAll();
    }

    private bool FindInventory()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<InventoryManager>();

        if (inventory == null)
        {
            Debug.LogWarning("[테스트] InventoryManager를 찾을 수 없습니다.");
            return false;
        }

        return true;
    }

    // F1: 목록 순서대로 하나씩 추가
    private void AddNext()
    {
        if (!FindInventory()) return;

        if (testIcons == null || testIcons.Length == 0)
        {
            Debug.LogWarning("[테스트] testIcons 목록이 비어 있습니다.");
            return;
        }

        Sprite icon = testIcons[nextIndex % testIcons.Length];

        if (AddTrash(icon))
            nextIndex++;
    }

    // F2: 목록 전체 추가 (인벤토리가 가득 차면 멈춤)
    private void AddAll()
    {
        if (!FindInventory()) return;

        if (testIcons == null || testIcons.Length == 0)
        {
            Debug.LogWarning("[테스트] testIcons 목록이 비어 있습니다.");
            return;
        }

        foreach (Sprite icon in testIcons)
        {
            if (!AddTrash(icon)) break;
        }
    }

    // F3: 인벤토리 비우기
    private void ClearAll()
    {
        if (!FindInventory()) return;

        // 슬롯 개수를 몰라도 되도록 넉넉하게 돌림 (범위 밖은 null로 반환됨)
        for (int i = 0; i < 64; i++)
        {
            if (inventory.GetItemAt(i) != null)
                inventory.RemoveItemAt(i);
        }

        nextIndex = 0;
        Debug.Log("[테스트] 인벤토리를 비웠습니다.");
    }

    private bool AddTrash(Sprite icon)
    {
        if (icon == null) return true; // 빈 칸은 건너뜀

        Item newItem = new Item();
        newItem.itemName = icon.name;
        newItem.itemIcon = icon;
        newItem.itemCount = 1;

        bool success = inventory.AddItem(newItem);
        Debug.Log(success
            ? $"[테스트] {newItem.itemName} 추가됨"
            : "[테스트] 인벤토리가 가득 찼습니다.");

        return success;
    }
#endif
}