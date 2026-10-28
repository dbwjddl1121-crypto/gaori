
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
    public string potionName = "체력 포션";
    public float healAmount = 15f;

    // 퀵슬롯 표시용 복사본이면 true
    [HideInInspector] public bool isMirror = false;

    // 슬롯 내용이 바뀌면 퀵슬롯 갱신
    public static event Action OnAnySlotChanged;

    private Item currentItem;
    public Item CurrentItem => currentItem;

    private bool hovering = false;

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

    // 마우스가 슬롯에 올라오면 툴팁 표시
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
