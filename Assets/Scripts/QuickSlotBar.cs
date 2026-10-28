
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuickSlotBar : MonoBehaviour
{
    [Header("연결")]
    public InventoryManager inventory;
    public Transform quickSlotContainer;
    public RectTransform selectionFrame;
    public GameObject quickSlotFrame;

    [Header("키 설정")]
    public KeyCode useKey = KeyCode.Q;
    public bool useMouseWheel = true;

    [Header("페이지 전환")]
    public KeyCode nextPageKey = KeyCode.Tab;
    public KeyCode previousPageKey = KeyCode.Tab;

    private List<Slot> quickSlots = new List<Slot>();
    private bool refreshing = false;
    private int selectedIndex = 0;
    private bool barHidden = false;

    // 0 = 1페이지, 1 = 2페이지
    private int currentPage = 0;

    IEnumerator Start()
    {
        foreach (Transform child in quickSlotContainer)
        {
            Slot slot = child.GetComponent<Slot>();
            if (slot != null)
            {
                slot.isMirror = true;
                quickSlots.Add(slot);
            }
        }

        if (selectionFrame != null)
            selectionFrame.gameObject.SetActive(false);

        Refresh();

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
        if (barHidden) return;

        // 페이지 전환: E = 다음 페이지, R = 이전 페이지
        if (Input.GetKeyDown(nextPageKey))
        {
            ChangePage(currentPage == 0 ? 1 : -1);
        }

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            HideSelectionFrame();
        }

        // 숫자키 1~8
        for (int i = 0; i < quickSlots.Count && i < 8; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                bool alreadySelected =
                    IsFrameVisible() && selectedIndex == i;

                Select(i);

                if (alreadySelected && inventory != null)
                    inventory.UseItemAt(GetInventoryIndex(i));
            }
        }

        // 마우스 휠로 현재 페이지의 퀵슬롯 선택
        if (useMouseWheel && quickSlots.Count > 0)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (scroll > 0f)
                Select((selectedIndex - 1 + quickSlots.Count) % quickSlots.Count);
            else if (scroll < 0f)
                Select((selectedIndex + 1) % quickSlots.Count);
        }

        // Q 키로 선택한 아이템 사용
        if (Input.GetKeyDown(useKey))
        {
            if (IsFrameVisible())
            {
                if (inventory != null)
                    inventory.UseItemAt(GetInventoryIndex(selectedIndex));
            }
            else
            {
                ShowSelectionFrame();
            }
        }
    }

    // 현재 페이지의 퀵슬롯 번호를 인벤토리 전체 번호로 변환
    private int GetInventoryIndex(int slotIndex)
    {
        return currentPage * 8 + slotIndex;
    }

    // 페이지 변경
    public void ChangePage(int direction)
    {
        int newPage = currentPage + direction;

        if (newPage < 0 || newPage > 1)
            return;

        currentPage = newPage;
        selectedIndex = 0;

        HideSelectionFrame();
        Refresh();

        Debug.Log($"퀵슬롯 {currentPage + 1}페이지");
    }

    public int GetSelectedInventoryIndex()
    {
        return GetInventoryIndex(selectedIndex);
    }

    public int GetCurrentPage()
    {
        return currentPage;
    }

    bool IsFrameVisible()
    {
        return selectionFrame != null &&
               selectionFrame.gameObject.activeSelf;
    }

    void HideSelectionFrame()
    {
        if (selectionFrame != null &&
            selectionFrame.gameObject.activeSelf)
        {
            selectionFrame.gameObject.SetActive(false);
        }
    }

    public void SetBarVisible(bool visible)
    {
        barHidden = !visible;

        foreach (Slot slot in quickSlots)
            slot.gameObject.SetActive(visible);

        if (quickSlotFrame != null)
            quickSlotFrame.SetActive(visible);

        HideSelectionFrame();
    }

    void Select(int index)
    {
        if (quickSlots.Count == 0) return;

        selectedIndex = Mathf.Clamp(index, 0, quickSlots.Count - 1);
        ShowSelectionFrame();
    }

    void ShowSelectionFrame()
    {
        if (selectionFrame == null) return;

        if (!selectionFrame.gameObject.activeSelf)
            selectionFrame.gameObject.SetActive(true);

        UpdateSelectionFrame();
    }

    void UpdateSelectionFrame()
    {
        if (selectionFrame == null || quickSlots.Count == 0)
            return;

        selectionFrame.position = quickSlots[selectedIndex].transform.position;
    }

    public void Refresh()
    {
        if (inventory == null || refreshing) return;

        refreshing = true;

        for (int i = 0; i < quickSlots.Count; i++)
        {
            int inventoryIndex = GetInventoryIndex(i);
            Item item = inventory.GetItemAt(inventoryIndex);

            if (item != null)
                quickSlots[i].SetItem(item);
            else
                quickSlots[i].ClearSlot();
        }

        refreshing = false;
    }

    public bool IsItemSelected()
    {
        return IsFrameVisible();
    }

    public Item GetSelectedItem()
    {
        if (IsFrameVisible() &&
            selectedIndex >= 0 &&
            selectedIndex < quickSlots.Count &&
            inventory != null)
        {
            return inventory.GetItemAt(GetInventoryIndex(selectedIndex));
        }

        return null;
    }

    public int GetSelectedIndex()
    {
        return GetInventoryIndex(selectedIndex);
    }
}
