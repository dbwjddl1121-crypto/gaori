using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    public List<ShopItemData> items;      // 파는 물건 목록
    public GameObject itemSlotPrefab;     // 아이템 슬롯 프리팹
    public Transform itemListContainer;   // 슬롯이 들어갈 자리

    [Header("구매한 아이템이 들어갈 인벤토리")]
    public InventoryManager inventoryManager;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        foreach (var item in items)
        {
            GameObject obj = Instantiate(itemSlotPrefab, itemListContainer);
            obj.GetComponent<ItemSlot>().Setup(item);
        }
    }

    public void TryBuy(ShopItemData item)
    {
        if (inventoryManager == null)
        {
            Debug.LogWarning("ShopManager에 InventoryManager가 연결되지 않았어요!");
            return;
        }

        // 자리가 없으면 돈을 쓰기 전에 막기
        if (!inventoryManager.HasEmptySlot())
        {
            Debug.Log("인벤토리가 가득 찼어요!");

            // 인벤토리가 가득 찼을 때 오류 소리
            PlayError();

            // 인벤토리가 꽉 찼을 때 화면에 안내 문구 표시
            NoticeUI.Show(
                "인벤토리가 가득 찼습니다!",
                "Your inventory is full!",
                "Túi đồ đã đầy!"
            );
            return;
        }

        if (CashManager.Instance.SpendCash(item.price))
        {
            Item newItem = new Item();
            newItem.itemName = item.itemName;
            newItem.itemIcon = item.icon;
            newItem.itemCount = 1;

            inventoryManager.AddItem(newItem);
            Debug.Log(item.itemName + " 구매!");

            // 구매 성공 소리
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX("buy");
        }
        else
        {
            Debug.Log("돈이 부족해요!");

            // 돈이 부족할 때 오류 소리
            PlayError();

            NoticeUI.Show(
                "캐시가 부족합니다!",
                "Not enough cash!",
                "Không đủ tiền!"
            );
        }
    }

    void PlayError()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("error");
    }
}