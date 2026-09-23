using UnityEngine;

public class TrashClickPickup : MonoBehaviour
{
    [Header("수거 가능한 거리")]
    public float pickupRange = 1.5f;

    [Header("아이템 정보")]
    public string trashName = "쓰레기";

    private Transform player;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("TrashClickPickup: Player 태그를 찾을 수 없습니다.");
        }
    }

    private void OnMouseDown()
    {
        if (player == null)
        {
            Debug.LogWarning("TrashClickPickup: Player를 찾을 수 없습니다.");
            return;
        }

        float distance = Vector2.Distance(
            player.position,
            transform.position
        );

        if (distance > pickupRange)
        {
            Debug.Log("쓰레기가 너무 멀리 있습니다.");
            return;
        }

        // --- 1. 인벤토리에 아이템 추가하는 로직 추가 ---
        InventoryManager inventory = FindAnyObjectByType<InventoryManager>();

        if (inventory != null)
        {
            Item newItem = new Item();
            newItem.itemName = trashName;

            // 스프라이트 렌더러의 이미지를 아이콘으로 사용
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            newItem.itemIcon = sr != null ? sr.sprite : null;
            newItem.itemCount = 1;

            // 인벤토리에 넣기 시도
            bool isSuccess = inventory.AddItem(newItem);

            // 2. 인벤토리 추가에 성공했을 때만 쓰레기 삭제
            if (isSuccess)
            {
                Debug.Log("쓰레기 수거 및 인벤토리 추가 성공!");
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("인벤토리가 가득 찼습니다!");
            }
        }
        else
        {
            Debug.LogWarning("TrashClickPickup: 씬에서 InventoryManager를 찾지 못했습니다!");
        }
    }
}