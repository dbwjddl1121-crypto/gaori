using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    public List<ShopItemData> items;      // 파는 물건 목록
    public GameObject itemSlotPrefab;     // 아이템 슬롯 프리팹
    public Transform itemListContainer;   // 슬롯이 들어갈 자리

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
        if (CashManager.Instance.SpendCash(item.price))
        {
            Debug.Log(item.itemName + " 구매!");
        }
        else
        {
            Debug.Log("돈이 부족해요!");
        }
    }
}