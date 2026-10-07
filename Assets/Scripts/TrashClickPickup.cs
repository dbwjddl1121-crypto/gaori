using UnityEngine;

public class TrashClickPickup : MonoBehaviour
{
    [Header("수거 가능한 거리")]
    public float pickupRange = 1.5f;

    [Header("아이템 정보")]
    public string trashName = "쓰레기";

    private Transform player;
    private string myID;

    void Start()
    {
        // 시작 위치 기준으로 ID 만들기
        myID = MakeID();

        // 이미 수거한 쓰레기라면 바로 삭제
        if (CollectedRecord.Has(myID))
        {
            Destroy(gameObject);
            return;
        }

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

    string MakeID()
    {
        Vector3 p = transform.position;
        return gameObject.scene.name + "_" + gameObject.name + "_" + Mathf.RoundToInt(p.x * 10) + "_" + Mathf.RoundToInt(p.y * 10);
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

        InventoryManager inventory = FindAnyObjectByType<InventoryManager>();

        if (inventory != null)
        {
            Item newItem = new Item();
            newItem.itemName = trashName;

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            newItem.itemIcon = sr != null ? sr.sprite : null;
            newItem.itemCount = 1;

            bool isSuccess = inventory.AddItem(newItem);

            if (isSuccess)
            {
                Debug.Log("쓰레기 수거 및 인벤토리 추가 성공!");
                RunStats.trashCount++;
                CollectedRecord.Add(myID);   // 수거 기록 저장
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