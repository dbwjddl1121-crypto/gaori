using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuickSlotBar : MonoBehaviour
{
    [Header("연결")]
    public InventoryManager inventory;      // 인벤토리 매니저
    public Transform quickSlotContainer;    // 퀵슬롯 8개가 들어있는 QuickSlotBar
    public RectTransform selectionFrame;    // 선택 표시 테두리

    [Header("키 설정")]
    public KeyCode useKey = KeyCode.Q;      // 선택한 칸 아이템 사용 키
    public bool useMouseWheel = true;       // 마우스 휠로 선택 변경

    private List<Slot> quickSlots = new List<Slot>();
    private bool refreshing = false;
    private int selectedIndex = 0;
    private bool barHidden = false;

    IEnumerator Start()
    {
        foreach (Transform child in quickSlotContainer)
        {
            Slot slot = child.GetComponent<Slot>();
            if (slot != null)
            {
                slot.isMirror = true; // 복사본 표시 (우클릭 사용 방지)
                quickSlots.Add(slot);
            }
        }

        // 처음엔 테두리를 숨겨둠
        if (selectionFrame != null) selectionFrame.gameObject.SetActive(false);

        Refresh();

        // 레이아웃이 확정될 때까지 한 프레임 기다린 뒤 위치 갱신
        yield return null;
        Canvas.ForceUpdateCanvases();
        UpdateSelectionFrame();
    }

    void OnEnable()
    {
        Slot.OnAnySlotChanged += Refresh;
    }

    void OnDisable()
    {
        Slot.OnAnySlotChanged -= Refresh;
    }

    void Update()
    {
        // 인벤토리가 열려 있으면 입력을 막음
        if (barHidden) return;

        // 마우스 클릭 → 테두리 숨김 (선택 번호는 유지)
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            HideSelectionFrame();
        }

        // 숫자키 1~8 → 처음 누르면 선택, 이미 선택된 칸을 다시 누르면 사용
        for (int i = 0; i < quickSlots.Count && i < 8; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                bool alreadySelected = IsFrameVisible() && selectedIndex == i;

                Select(i);

                if (alreadySelected)
                    inventory.UseItemAt(i);
            }
        }

        // 마우스 휠로 선택 이동 (선택 사항)
        if (useMouseWheel && quickSlots.Count > 0)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0f) Select((selectedIndex - 1 + quickSlots.Count) % quickSlots.Count);
            else if (scroll < 0f) Select((selectedIndex + 1) % quickSlots.Count);
        }

        // 사용 키(Q)
        if (Input.GetKeyDown(useKey))
        {
            if (IsFrameVisible())
            {
                // 테두리가 보이는 상태 → 선택한 칸 사용
                inventory.UseItemAt(selectedIndex);
            }
            else
            {
                // 테두리가 숨겨진 상태 → 먼저 선택 칸만 보여줌 (실수 방지)
                // 바로 사용하고 싶으면 아래 줄 뒤에 inventory.UseItemAt(selectedIndex); 를 추가하세요
                ShowSelectionFrame();
            }
        }
    }

    bool IsFrameVisible()
    {
        return selectionFrame != null && selectionFrame.gameObject.activeSelf;
    }

    void HideSelectionFrame()
    {
        if (selectionFrame != null && selectionFrame.gameObject.activeSelf)
            selectionFrame.gameObject.SetActive(false);
    }

    // 퀵슬롯 칸들과 선택 테두리를 보이거나 숨김 (Inventory.cs에서 호출)
    public void SetBarVisible(bool visible)
    {
        barHidden = !visible;

        foreach (Slot slot in quickSlots)
            slot.gameObject.SetActive(visible);

        // 인벤토리를 열었다 닫으면 테두리는 숨긴 상태로 시작
        HideSelectionFrame();
    }

    void Select(int index)
    {
        if (quickSlots.Count == 0) return;
        selectedIndex = Mathf.Clamp(index, 0, quickSlots.Count - 1);
        ShowSelectionFrame();
    }

    // 테두리를 켜고 선택된 칸 위치로 이동
    void ShowSelectionFrame()
    {
        if (selectionFrame == null) return;
        if (!selectionFrame.gameObject.activeSelf)
            selectionFrame.gameObject.SetActive(true);
        UpdateSelectionFrame();
    }

    // 선택 테두리를 선택된 칸 위치로 이동
    void UpdateSelectionFrame()
    {
        if (selectionFrame == null || quickSlots.Count == 0) return;
        selectionFrame.position = quickSlots[selectedIndex].transform.position;
    }

    // 인벤토리 앞 8칸을 퀵슬롯에 복사해서 표시
    public void Refresh()
    {
        if (inventory == null || refreshing) return;
        refreshing = true; // 무한 루프 방지

        for (int i = 0; i < quickSlots.Count; i++)
        {
            Item item = inventory.GetItemAt(i);

            if (item != null)
                quickSlots[i].SetItem(item);
            else
                quickSlots[i].ClearSlot();
        }

        refreshing = false;
    }
}