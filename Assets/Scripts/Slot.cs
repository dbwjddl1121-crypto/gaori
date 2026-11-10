using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class Slot : MonoBehaviour,
    IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI 컴포넌트")]
    public Image iconImage;
    public TextMeshProUGUI countText;

    [Header("체력 포션 설정")]
    public string potionName = "체력 포션";
    public float healAmount = 15f;

    // 퀵슬롯 표시용 복사본이면 true
    [HideInInspector] public bool isMirror = false;

    // 슬롯 내용이 바뀌면 퀵슬롯 갱신
    public static event Action OnAnySlotChanged;

    private Item currentItem;
    public Item CurrentItem => currentItem;

    private bool hovering = false;

    // 드래그 중 마우스를 따라다니는 아이콘
    private GameObject dragGhost;
    private Canvas dragCanvas;

    void Awake()
    {
        if (iconImage != null)
        {
            iconImage.type = Image.Type.Simple;
            iconImage.preserveAspect = true;
        }
    }

    public void SetItem(Item item)
    {
        currentItem = item;

        if (iconImage != null)
        {
            if (item != null && item.itemIcon != null)
            {
                iconImage.sprite = item.itemIcon;
                iconImage.preserveAspect = true;
                iconImage.gameObject.SetActive(true);

                Color color = iconImage.color;
                color.a = 1f;
                iconImage.color = color;
            }
            else
            {
                iconImage.sprite = null;
                iconImage.gameObject.SetActive(false);
            }
        }

        if (countText != null)
        {
            countText.text = item != null && item.itemCount > 1
                ? item.itemCount.ToString()
                : "";
        }

        OnAnySlotChanged?.Invoke();
    }

    public void ClearSlot()
    {
        currentItem = null;

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.gameObject.SetActive(false);

            Color color = iconImage.color;
            color.a = 1f;
            iconImage.color = color;
        }

        if (countText != null)
        {
            countText.text = "";
        }

        HideTooltip();
        OnAnySlotChanged?.Invoke();
    }

    // ───────── 툴팁 ─────────

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isMirror) return;

        hovering = true;

        if (currentItem != null && ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.Show(currentItem);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    void OnDisable()
    {
        HideTooltip();
        DestroyGhost();
    }

    void HideTooltip()
    {
        if (!hovering) return;

        hovering = false;

        if (ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.Hide();
        }
    }

    // ───────── 드래그 앤 드롭 ─────────

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isMirror) return;
        if (currentItem == null || iconImage == null || iconImage.sprite == null) return;

        HideTooltip();

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        dragCanvas = canvas.rootCanvas;

        // 마우스를 따라다닐 복사 아이콘 만들기
        dragGhost = new GameObject("DragGhost");
        dragGhost.transform.SetParent(dragCanvas.transform, false);
        dragGhost.transform.SetAsLastSibling();

        Image ghostImage = dragGhost.AddComponent<Image>();
        ghostImage.sprite = iconImage.sprite;
        ghostImage.preserveAspect = true;
        ghostImage.raycastTarget = false; // 드롭 대상을 가리지 않게

        RectTransform ghostRect = dragGhost.GetComponent<RectTransform>();
        ghostRect.sizeDelta = iconImage.rectTransform.rect.size;

        // 원래 칸의 아이콘은 반투명하게
        Color c = iconImage.color;
        c.a = 0.4f;
        iconImage.color = c;

        MoveGhost(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragGhost == null) return;
        MoveGhost(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DestroyGhost();

        // 슬롯 밖에 놓았거나 취소되면 아이콘 원래대로
        if (iconImage != null)
        {
            Color c = iconImage.color;
            c.a = 1f;
            iconImage.color = c;
        }
    }

    // 다른 칸 위에 놓였을 때: 두 칸의 아이템 맞바꾸기
    public void OnDrop(PointerEventData eventData)
    {
        if (isMirror) return;

        GameObject dragged = eventData.pointerDrag;
        if (dragged == null) return;

        InventoryManager manager = FindFirstObjectByType<InventoryManager>();

        // 장비칸에서 끌어온 경우
        EquipSlot equip = dragged.GetComponentInParent<EquipSlot>();
        if (equip != null)
        {
            Item equipItem = equip.CurrentItem;
            if (equipItem == null) return;

            Item myItem = this.currentItem;

            // 내 칸에 있던 아이템이 장비칸에 못 들어가는 종류면 취소
            if (myItem != null && !equip.CanEquip(myItem))
            {
                Debug.Log("그 아이템은 이 장비칸에 들어갈 수 없어요: " + myItem.itemName);
                return;
            }

            this.SetItem(equipItem);
            equip.SetEquipped(myItem); // myItem이 null이면 장비칸 비워짐

            if (manager != null) manager.SaveToStore();
            return;
        }

        // 인벤토리 칸에서 끌어온 경우
        Slot from = dragged.GetComponentInParent<Slot>();
        if (from == null || from == this || from.isMirror) return;
        if (from.CurrentItem == null) return;

        Item fromItem = from.CurrentItem;
        Item toItem = this.currentItem;

        this.SetItem(fromItem);

        if (toItem != null)
            from.SetItem(toItem);
        else
            from.ClearSlot();

        if (manager != null) manager.SaveToStore();
    }

    void MoveGhost(PointerEventData eventData)
    {
        if (dragGhost == null || dragCanvas == null) return;

        if (dragCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            dragGhost.transform.position = eventData.position;
        }
        else
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                dragCanvas.transform as RectTransform,
                eventData.position,
                dragCanvas.worldCamera,
                out Vector3 worldPos);
            dragGhost.transform.position = worldPos;
        }
    }

    void DestroyGhost()
    {
        if (dragGhost != null)
        {
            Destroy(dragGhost);
            dragGhost = null;
        }
    }

    // ───────── 아이템 사용 ─────────

    // 인벤토리 슬롯 우클릭: 체력 포션 사용
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        if (isMirror) return;

        UseItem();
    }

    // 기존 체력 포션 사용 기능 유지
    public void UseItem()
    {
        if (currentItem == null) return;

        Debug.Log("사용한 아이템 이름: " + currentItem.itemName);

        if (currentItem.itemName != potionName) return;

        if (!PlayerVitals.Heal(healAmount))
        {
            Debug.Log("지금은 체력 포션을 쓸 수 없어요 (체력이 가득 참)");
            return;
        }

        currentItem.itemCount--;

        if (currentItem.itemCount <= 0)
        {
            ClearSlot();
        }
        else
        {
            SetItem(currentItem);
        }

        InventoryManager manager = FindFirstObjectByType<InventoryManager>();

        if (manager != null)
        {
            manager.SaveToStore();
        }
    }

    // 분리수거 전용: 선택한 슬롯의 아이템 1개 제거
    public void RemoveOneItem()
    {
        if (currentItem == null) return;

        if (currentItem.itemCount > 1)
        {
            currentItem.itemCount--;
            SetItem(currentItem);
        }
        else
        {
            ClearSlot();
        }
    }
}