using UnityEngine;

// 테스트용: 단축키로 인벤토리에 쓰레기 추가 (에디터에서만 동작)
public class InventoryDebug : MonoBehaviour
{
    [Header("테스트용 아이템 아이콘")]
    public Sprite plasticBottleIcon;   // 생수병
    public Sprite brokenGlassIcon;     // 깨진 유리
    public Sprite colaCanIcon;         // 콜라캔

    private InventoryManager inventory;

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) AddTrash("생수병", plasticBottleIcon);
        if (Input.GetKeyDown(KeyCode.F2)) AddTrash("깨진 유리", brokenGlassIcon);
        if (Input.GetKeyDown(KeyCode.F3)) AddTrash("콜라캔", colaCanIcon);
    }

    private void AddTrash(string itemName, Sprite icon)
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<InventoryManager>();

        if (inventory == null)
        {
            Debug.LogWarning("[테스트] InventoryManager를 찾을 수 없습니다.");
            return;
        }

        Item newItem = new Item();
        newItem.itemName = itemName;
        newItem.itemIcon = icon;
        newItem.itemCount = 1;

        bool success = inventory.AddItem(newItem);
        Debug.Log(success
            ? $"[테스트] {itemName} 추가됨"
            : "[테스트] 인벤토리가 가득 찼습니다.");
    }
#endif
}