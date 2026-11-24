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

    const string SaveKey = "EquippedItems";

    // 장착 상태 (씬이 바뀌어도, 게임을 껐다 켜도 유지)
    private static Dictionary<int, Item> saved = new Dictionary<int, Item>();
    private static bool loaded = false;

    // 아이템 이름 → 아이콘 (불러올 때 아이콘을 다시 붙이기 위한 표)
    private static Dictionary<string, Sprite> iconCache;

    public static event Action OnEquipChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatic()
    {
        saved = new Dictionary<int, Item>();
        loaded = false;
        iconCache = null;
    }

    private Item currentItem;
    public Item CurrentItem => currentItem;

    private GameObject dragGhost;
    private Canvas dragCanvas;

    // ───────── 저장 / 불러오기 (PlayerPrefs) ─────────

    static Sprite FindIcon(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return null;

        if (iconCache == null)
        {
            iconCache = new Dictionary<string, Sprite>();

            // Resources 폴더 안의 ShopItemData에서 아이콘을 모음
            ShopItemData[] shopItems = Resources.LoadAll<ShopItemData>("");
            foreach (ShopItemData s in shopItems)
            {
                if (s != null && !string.IsNullOrEmpty(s.itemName) && s.icon != null)
                    iconCache[s.itemName] = s.icon;
            }
        }

        Sprite icon;
        return iconCache.TryGetValue(itemName, out icon) ? icon : null;
    }

    // 처음 쓸 때 저장된 장비를 한 번만 불러옴
    static void EnsureLoaded()
    {
        if (loaded) return;
        loaded = true;

        saved.Clear();

        string raw = PlayerPrefs.GetString(SaveKey, "");
        if (string.IsNullOrEmpty(raw)) return;

        foreach (string line in raw.Split('\n'))
        {
            if (string.IsNullOrEmpty(line)) continue;

            // 한 줄 형식: 칸번호 \t 아이템이름 \t 개수
            string[] parts = line.Split('\t');
            if (parts.Length < 2) continue;

            int id;
            if (!int.TryParse(parts[0], out id)) continue;

            int count = 1;
            if (parts.Length > 2 && !int.TryParse(parts[2], out count))
                count = 1;

            Item item = new Item();
            item.itemName = parts[1];
            item.itemCount = count;
            item.itemIcon = FindIcon(parts[1]);

            if (item.itemIcon == null)
                Debug.LogWarning("[EquipSlot] 아이콘을 찾지 못했어요 → " + parts[1] +
                                 " (ShopItemData를 Resources 폴더로 옮겨 주세요)");

            saved[id] = item;
        }
    }

    static void SaveToPrefs()
    {
        List<string> lines = new List<string>();

        foreach (var pair in saved)
        {
            if (pair.Value == null) continue;
            lines.Add(pair.Key + "\t" + pair.Value.itemName + "\t" + pair.Value.itemCount);
        }

        PlayerPrefs.SetString(SaveKey, string.Join("\n", lines.ToArray()));
        PlayerPrefs.Save();
    }

    // 새 게임 시작용: 장착 상태를 모두 지움 (저장된 기록도 함께 지움)
    public static void ResetAll()
    {
        saved = new Dictionary<int, Item>();
        loaded = true;

        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();

        OnEquipChanged?.Invoke();
    }

    // ───────── 다른 스크립트에서 쓰는 함수 ─────────

    // 해당 이름의 아이템이 장착되어 있는지
    public static bool IsEquipped(string itemName)
    {
        EnsureLoaded();

        foreach (var pair in saved)
        {
            if (pair.Value != null && pair.Value.itemName == itemName)
                return true;
        }
        return false;
    }

    public static Item GetEquipped(string itemName)
    {
        EnsureLoaded();

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
        EnsureLoaded();

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
        EnsureLoaded();

        currentItem = item;
        saved[slotId] = item;
        SaveToPrefs();

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

            // 장착할 수 없는 아이템일 때 오류 소리
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX("error");

            NoticeUI.Show(
                "작살, 산소통, 손전등만 장착할 수 있어요.",
                "Only the harpoon, oxygen tank, and flashlight can be equipped.",
                "Chỉ có thể trang bị lao, bình oxy và đèn pin."
            );
            return;
        }

        Item oldItem = currentItem;

        SetEquipped(fromItem);

        // 장착 소리
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("equip");

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