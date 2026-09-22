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

        float distance = Vector2.Distance(player.position, transform.position);

        if (distance > pickupRange)
        {
            Debug.Log("쓰레기가 너무 멀리 있습니다.");
            return;
        }

        InventoryManager inventory = FindAnyObjectByType<InventoryManager>();

        if (inventory != null)
        {
            Item newItem = new Item();
            newItem.itemName = trashName;
            newItem.itemCount = 1;

            // 바닥에 있는 쓰레기의 SpriteRenderer에서 이미지를 가져옵니다.
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                newItem.itemIcon = sr.sprite;
                Debug.Log("쓰레기 이미지 가져오기 성공: " + sr.sprite.name);
            }
            else
            {
                Debug.LogWarning("TrashClickPickup: 쓰레기에 SpriteRenderer나 Sprite가 없습니다!");
            }

            // 인벤토리에 넣기 시도
            bool isSuccess = inventory.AddItem(newItem);

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