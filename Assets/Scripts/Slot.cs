using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class Slot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 컴포넌트")]
    public Image iconImage;
    public TextMeshProUGUI countText;

    [Header("체력 포션 설정")]
    public string potionName = "체력 포션";  // 상점 아이템 이름과 똑같이
    public float healAmount = 15f;           // 한 번에 회복되는 체력

    // 퀵슬롯 표시용 복사본 슬롯이면 true (QuickSlotBar가 자동으로 설정)
    [HideInInspector] public bool isMirror = false;

    // 어떤 슬롯이든 내용이 바뀌면 호출됨 (퀵슬롯 갱신용)
    public static event Action OnAnySlotChanged;

    private Item currentItem;
    public Item CurrentItem => currentItem;

    private bool hovering = false;

    void Awake()
    {
        // 아이콘이 칸 모양대로 늘어나지 않고 원래 비율을 유지하게 함
        if (iconImage != null)
        {
            iconImage.type = Image.Type.Simple;
            iconImage.preserveAspect = true;
        }
    }

    public void SetItem(Item item)
    {
        currentItem = item;

        if (iconImage != null && item != null && item.itemIcon != null)
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
            Debug.LogWarning("Slot: 아이콘을 설정할 수 없습니다. 아이템 또는 이미지가 비어있습니다.");
        }

        if (countText != null && item != null)
        {
            countText.text = item.itemCount > 1 ? item.itemCount.ToString() : "";
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

        // 마우스가 올라가 있던 칸이 비워지면 툴팁도 끔
        HideTooltip();

        OnAnySlotChanged?.Invoke();
    }

    // 마우스가 칸 위에 올라옴 → 툴팁 표시 (퀵슬롯 복사본은 제외)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isMirror) return;
        hovering = true;

        if (currentItem != null && ItemTooltip.Instance != null)
            ItemTooltip.Instance.Show(currentItem);
    }

    // 마우스가 칸에서 벗어남 → 툴팁 숨김
    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    // 인벤토리가 닫혀서 칸이 꺼질 때도 툴팁이 남지 않게 함
    void OnDisable()
    {
        HideTooltip();
    }

    void HideTooltip()
    {
        if (!hovering) return;
        hovering = false;
        if (ItemTooltip.Instance != null) ItemTooltip.Instance.Hide();
    }

    // 슬롯 우클릭 → 체력 포션 사용 (퀵슬롯 복사본은 무시)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;
        if (isMirror) return;
        UseItem();
    }

    // 아이템 사용 (우클릭, 숫자키 둘 다 이 함수를 씀)
    public void UseItem()
    {
        if (currentItem == null) return;

        Debug.Log("사용한 아이템 이름: " + currentItem.itemName);
        if (currentItem.itemName != potionName) return;

        // 체력이 가득 차 있으면 사용하지 않음
        if (!PlayerVitals.Heal(healAmount))
        {
            Debug.Log("지금은 체력 포션을 쓸 수 없어요 (체력이 가득 참)");
            return;
        }

        // 포션 1개 소모
        currentItem.itemCount--;
        if (currentItem.itemCount <= 0)
            ClearSlot();
        else
            SetItem(currentItem);

        // 변경된 인벤토리를 저장소에 기록
        InventoryManager manager = FindFirstObjectByType<InventoryManager>();
        if (manager != null) manager.SaveToStore();
    }
}