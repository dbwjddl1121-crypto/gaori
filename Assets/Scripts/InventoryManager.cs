using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public Transform slotContainer; // 슬롯들이 들어있는 SlotContainer 연결
    private List<Slot> slots = new List<Slot>();

    void Start()
    {
        // 컨테이너 안에 있는 모든 슬롯 컴포넌트를 가져와 리스트에 담습니다.
        foreach (Transform child in slotContainer)
        {
            Slot slot = child.GetComponent<Slot>();
            if (slot != null)
            {
                slots.Add(slot);
            }
        }
    }

    // 외부에서 아이템을 획득했을 때 호출하는 함수
    public bool AddItem(Item newItem)
    {
        // 비어있는 슬롯을 찾아 아이템 배치
        foreach (Slot slot in slots)
        {
            // 슬롯의 데이터가 비어있다면 빈 슬롯으로 판정
            if (slot.CurrentItem == null)
            {
                slot.SetItem(newItem);
                return true; // 추가 성공
            }
        }

        Debug.Log("인벤토리가 가득 찼습니다!");
        return false; // 인벤토리 가득 참
    }
}