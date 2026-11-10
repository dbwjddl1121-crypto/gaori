using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Collections.Generic;

public class EquipSlot : MonoBehaviour,
    IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("장비칸 설정")]
    public int slotId = 0;                 // 장비칸마다 다르게 (0, 1)

    [Header("UI 컴포넌트")]
    public Image iconImage;                // 칸 안의 아이콘용 자식 Image
    public TextMeshProUGUI countText;      // 선택사항

    // 씬이 바뀌어도 장착 상태 유지
    private static Dictionary<int, Item> saved = new Dictionary<int, Item>();

    public static event Action OnEquipChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic()
    {
        saved = new Dictionary<int, Item>();
    }

    private Item currentItem;
    public Item CurrentItem => currentItem;

    private GameObject dragGhost;
    private Canvas dragCanvas;

    // ───────── 다른 스크립트에서 쓰는 함수 ─────────

    // 해당 이름의 아이템이 장착되어 있는지
    public static bool IsEquipped(string itemName)
    {
        foreach (var pair in saved)
        {
            if (pair.Value != null && pair.Value.itemName == itemName)
                return true;
        }
        return false;
    }

    public static Item GetEquipped(string itemName)
    {
        foreach (var pair in saved)
        {
            if (pair.Value != null && pair.Value.itemName == itemName)
                return pair.Value;
        }
        return null;
    }

    // 장비칸에 넣을 수 있는 아이템 이름인지 (작살, 산소통, 손전등만 가능)
    public static bool IsEquipItemName(string itemName)
    {
        return itemName == EquipEffects.HarpoonName ||
               itemName == EquipEffects.OxygenTankName ||
               itemName == EquipEffects.FlashlightName;
    }

    // ───────── 초기화 ─────────

    void Start()
    {
        if (saved.TryGetValue(slotId, out Item item) && item != null)
            SetEquipped(item);
        else
            SetEquipped(null);
    }

    public bool CanEquip(Item item)
    {
        if (item == null) return false;
        return IsEquipItemName(item.itemName);
    }

    // item이 null이면 비우기
    public void SetEquipped(Item item)
    {
        currentItem = item;
        saved[slotId] = item;

        if (iconImage != null)
        {
            if (item != null && item.itemIcon != null)
            {
                iconImage.sprite = item.itemIcon;
                iconImage.preserveAspect = true;
                iconImage.gameObject.SetActive(true);

                Color c = iconImage.color;
                c.a = 1f;
                iconImage.color = c;
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

        OnEquipChanged?.Invoke();
    }

    // ───────── 인벤토리 칸에서 끌어다 놓기 ─────────

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dragged = eventData.pointerDrag;
        if (dragged == null) return;

        Slot from = dragged.GetComponentInParent<Slot>();
        if (from == null || from.isMirror || from.CurrentItem == null) return;

        Item fromItem = from.CurrentItem;

        if (!CanEquip(fromItem))
        {
            Debug.Log("이 칸에는 장착할 수 없는 아이템이에요: " + fromItem.itemName);
            NoticeUI.Show(
                "작살, 산소통, 손전등만 장착할 수 있어요.",
                "Only the harpoon, oxygen tank, and flashlight can be equipped.",
                "Chỉ có thể trang bị lao, bình oxy và đèn pin."
            );
            return;
        }

        Item oldItem = currentItem;

        SetEquipped(fromItem);

        if (oldItem != null)
            from.SetItem(oldItem);
        else
            from.ClearSlot();

        InventoryManager manager = FindFirstObjectByType<InventoryManager>();
        if (manager != null)
            manager.SaveToStore();
    }

    // ───────── 장비칸에서 끌어내기 ─────────

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null || iconImage == null || iconImage.sprite == null) return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        dragCanvas = canvas.rootCanvas;

        dragGhost = new GameObject("DragGhost");
        dragGhost.transform.SetParent(dragCanvas.transform, false);
        dragGhost.transform.SetAsLastSibling();

        Image ghostImage = dragGhost.AddComponent<Image>();
        ghostImage.sprite = iconImage.sprite;
        ghostImage.preserveAspect = true;
        ghostImage.raycastTarget = false;

        RectTransform ghostRect = dragGhost.GetComponent<RectTransform>();
        ghostRect.sizeDelta = iconImage.rectTransform.rect.size;

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

        if (iconImage != null)
        {
            Color c = iconImage.color;
            c.a = 1f;
            iconImage.color = c;
        }
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

    void OnDisable()
    {
        DestroyGhost();
    }
}