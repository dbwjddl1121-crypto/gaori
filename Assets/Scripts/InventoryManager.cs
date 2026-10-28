
using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public Transform slotContainer;
    private List<Slot> slots = new List<Slot>();

    void Start()
    {
        if (slotContainer == null)
        {
            Debug.LogError("InventoryManager: Slot Container가 연결되지 않았습니다!");
            return;
        }

        slots.Clear();

        foreach (Transform child in slotContainer)
        {
            Slot slot = child.GetComponent<Slot>();

            if (slot != null)
            {
                slots.Add(slot);
            }
        }

        List<Item> saved = InventoryStore.Load();

        for (int i = 0; i < slots.Count && i < saved.Count; i++)
        {
            if (saved[i] != null)
            {
                slots[i].SetItem(saved[i]);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }

        InventoryStore.lockSave = false;
    }

    // 해당 슬롯의 아이템 반환
    public Item GetItemAt(int index)
    {
        if (index < 0 || index >= slots.Count)
            return null;

        return slots[index].CurrentItem;
    }

    // 기존 아이템 사용 기능 유지
    public void UseItemAt(int index)
    {
        if (index < 0 || index >= slots.Count)
            return;

        slots[index].UseItem();
    }

    // 분리수거 전용: 지정한 슬롯의 아이템 1개 제거
    public void RemoveItemAt(int index)
    {
        if (index < 0 || index >= slots.Count)
        {
            Debug.LogWarning("분리수거 실패: 슬롯 번호가 올바르지 않습니다.");
            return;
        }

        if (slots[index].CurrentItem == null)
        {
            Debug.LogWarning("분리수거 실패: 해당 슬롯에 아이템이 없습니다.");
            return;
        }

        slots[index].RemoveOneItem();

        SaveToStore();

        Debug.Log("분리수거 후 인벤토리를 저장했습니다.");
    }

    // 빈 슬롯 확인
    public bool HasEmptySlot()
    {
        foreach (Slot slot in slots)
        {
            if (slot.CurrentItem == null)
                return true;
        }

        return false;
    }

    // 아이템 획득
    public bool AddItem(Item newItem)
    {
        foreach (Slot slot in slots)
        {
            if (slot.CurrentItem == null)
            {
                slot.SetItem(newItem);
                SaveToStore();

                TrashCollection.Record(newItem.itemName);

                return true;
            }
        }

        Debug.Log("인벤토리가 가득 찼습니다!");
        return false;
    }

    // 현재 인벤토리 저장
    public void SaveToStore()
    {
        List<Item> items = new List<Item>();

        foreach (Slot slot in slots)
        {
            items.Add(slot.CurrentItem);
        }

        InventoryStore.Save(items);
    }

    void OnDestroy()
    {
        if (slots.Count > 0)
        {
            SaveToStore();
        }
    }
}
